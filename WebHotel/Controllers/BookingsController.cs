using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebHotel.Data;
using WebHotel.Models;
// add this to support the type SqliteParameter 
using Microsoft.Data.Sqlite;

namespace WebHotel.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        [Authorize(Roles = "Customers")]
        public async Task<IActionResult> Index (string sortOrder)
        {
            if (String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "room_asc";
            }
            var bookings = (IQueryable<Booking>)_context.Booking.Include(b => b.TheCustomer).Include(b => b.TheRoom).Where(b => b.CustomerEmail == User.Identity.Name);
            // Sort the Booking by specified order
            switch (sortOrder)
            {
                case "checkIn_asc":
                    bookings = bookings.OrderBy(m => m.CheckIn);
                    break;
                case "checkIn_desc":
                    bookings = bookings.OrderByDescending(m => m.CheckIn);
                    break;
                case "checkOut_asc":
                    bookings = bookings.OrderBy(m => m.CheckOut);
                    break;
                case "checkOut_desc":
                    bookings = bookings.OrderByDescending(m => m.CheckOut);
                    break;
                case "cost_asc":
                    bookings = bookings.OrderBy(m => m.Cost);
                    break;
                case "cost_desc":
                    bookings = bookings.OrderByDescending(m => m.Cost);
                    break;
                case "room_asc":
                    bookings = bookings.OrderBy(m => m.RoomID);
                    break;
                case "room_desc":
                    bookings = bookings.OrderByDescending(m => m.RoomID);
                    break;
            }
            ViewData["CheckInOrder"] = sortOrder != "checkIn_asc" ? "checkIn_asc" : "checkIn_desc";
            ViewData["CheckOutOrder"] = sortOrder != "checkOut_asc" ? "checkOut_asc" : "checkOut_desc";
            ViewData["NextCostOrder"] = sortOrder != "cost_asc" ? "cost_asc" : "cost_desc";
            ViewData["NextRoomOrder"] = sortOrder != "room_asc" ? "room_asc" : "room_desc";
            return View(await bookings.AsNoTracking().ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.TheCustomer)
                .Include(b => b.TheRoom)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        [Authorize(Roles = "Customers")]
        public IActionResult Create()
        {
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email");
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            var roId = new SqliteParameter("roId", booking.RoomID);
            var checkIn = new SqliteParameter("checkInS", booking.CheckIn);
            var checkOut = new SqliteParameter("checkOutS", booking.CheckOut);

            bool checkDates = booking.CheckIn < booking.CheckOut;

            var roomsAvailable = _context.Room.FromSql("select * from [Room] "
                    + "where [Room].ID = @roId and [Room].ID not in "
                    + "(select [Room].ID from [Room] inner join [Booking] on [Room].ID = [Booking].RoomID "
                    + "where @checkInS < [Booking].CheckOut and [Booking].CheckIn < @checkOutS)", roId, checkIn, checkOut)
                .Select(ro => new Room { ID = ro.ID, Level = ro.Level});

            var Rooms = await roomsAvailable.ToListAsync();


            if (ModelState.IsValid && Rooms.Count != 0 && checkDates)
            {
                // Shows only the current login user booking details
                string _email = User.FindFirst(ClaimTypes.Name).Value;
                booking.CustomerEmail = _email;

                var room = await _context.Room.SingleOrDefaultAsync(m => m.ID==booking.RoomID);
                // calculate the total cost of booking
                TimeSpan bookingDays = booking.CheckOut - booking.CheckIn;
                decimal bookedDays = (decimal)bookingDays.TotalDays;
                booking.Cost = room.Price * bookedDays;

                _context.Add(booking);
                await _context.SaveChangesAsync();

                ViewBag.RoomFloor = room.Level;

            }
            if (!checkDates)
            {
                ViewBag.DateError = "Sorry, check out date cannot be less than or the same as check in date";
            }
            else if(Rooms.Count == 0)
            {
                ViewBag.Error = "Sorry, This room is not available for your selected time period";
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID", booking.RoomID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID", booking.RoomID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            var roId = new SqliteParameter("roId", booking.RoomID);
            var checkIn = new SqliteParameter("checkInS", booking.CheckIn);
            var checkOut = new SqliteParameter("checkOutS", booking.CheckOut);
            var bookingId= new SqliteParameter("bookingId", id);

            if (id != booking.ID)
            {
                return NotFound();
            }

            bool checkDates = booking.CheckIn < booking.CheckOut;

            var roomsAvailable = _context.Room.FromSql("select * from [Room] "
                    + "where [Room].ID = @roId and [Room].ID not in "
                    + "(select [Room].ID from [Room] inner join [Booking] on [Room].ID = [Booking].RoomID "
                    + "where @checkInS < [Booking].CheckOut and [Booking].CheckIn < @checkOutS and [Booking].ID != @bookingId)", roId, checkIn, checkOut, bookingId)
                .Select(ro => new Room { ID = ro.ID, Level = ro.Level });

            var Rooms = await roomsAvailable.ToListAsync();

            if (ModelState.IsValid && Rooms.Count != 0 && checkDates)
            {
                
                try
                {

                    var room = await _context.Room.SingleOrDefaultAsync(m => m.ID == booking.RoomID);

                    TimeSpan bookingDays = booking.CheckOut - booking.CheckIn;
                    decimal bookedDays = (decimal)bookingDays.TotalDays;
                    booking.Cost = room.Price * bookedDays;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageBookings));
            }
            if (!checkDates)
            {
                ViewBag.DateError = "Sorry, check out date cannot be less than or the same as check in date";
            }
            else if (Rooms.Count == 0)
            {
                ViewBag.Error = "Sorry, This room is not available for your selected time period";
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID", booking.RoomID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.TheCustomer)
                .Include(b => b.TheRoom)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.ID == id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageBookings));
        }

        // GET: RoomID/CalStats
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalStats()
        {
            string _email = User.FindFirst(ClaimTypes.Name).Value;


            var list_Booking = _context.Booking.GroupBy(c => c.RoomID);

            var nameStats2 = list_Booking.Select(g => new CalStats { RoomID = g.Key.ToString(), NumberOfBookings = g.Count() });

            // pass the list of NameStatistic objects to view
            return View(await nameStats2.ToListAsync());
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.ID == id);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageBookings(string sortOrder)
        {
            var bookings = (IQueryable<Booking>)_context.Booking.Include(b => b.TheCustomer).Include(b => b.TheRoom);
            // Sort the Booking by specified order

            if (String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "surname_asc";
            }

            switch (sortOrder)
            {
                case "surname_asc":
                    bookings = bookings.OrderBy(m => m.TheCustomer.Surname);
                    break;
                case "surname_desc":
                    bookings = bookings.OrderByDescending(m => m.TheCustomer.Surname);
                    break;
                case "checkIn_asc":
                    bookings = bookings.OrderBy(m => m.CheckIn);
                    break;
                case "checkIn_desc":
                    bookings = bookings.OrderByDescending(m => m.CheckIn);
                    break;
                case "checkOut_asc":
                    bookings = bookings.OrderBy(m => m.CheckOut);
                    break;
                case "checkOut_desc":
                    bookings = bookings.OrderByDescending(m => m.CheckOut);
                    break;
                case "cost_asc":
                    bookings = bookings.OrderBy(m => m.Cost);
                    break;
                case "cost_desc":
                    bookings = bookings.OrderByDescending(m => m.Cost);
                    break;
                case "room_asc":
                    bookings = bookings.OrderBy(m => m.RoomID);
                    break;
                case "room_desc":
                    bookings = bookings.OrderByDescending(m => m.RoomID);
                    break;
            }
            ViewData["SurnameOrder"] = sortOrder != "surname_asc" ? "surname_asc" : "surname_desc";
            ViewData["CheckInOrder"] = sortOrder != "checkIn_asc" ? "checkIn_asc" : "checkIn_desc";
            ViewData["CheckOutOrder"] = sortOrder != "checkOut_asc" ? "checkOut_asc" : "checkOut_desc";
            ViewData["NextCostOrder"] = sortOrder != "cost_asc" ? "cost_asc" : "cost_desc";
            ViewData["NextRoomOrder"] = sortOrder != "room_asc" ? "room_asc" : "room_desc";
            return View(await bookings.AsNoTracking().ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminCreate()
        {
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email");
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCreate([Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            var roId = new SqliteParameter("roId", booking.RoomID);
            var checkIn = new SqliteParameter("checkInS", booking.CheckIn);
            var checkOut = new SqliteParameter("checkOutS", booking.CheckOut);

            bool checkDates = booking.CheckIn < booking.CheckOut;

            var roomsAvailable = _context.Room.FromSql("select * from [Room] "
                    + "where [Room].ID = @roId and [Room].ID not in "
                    + "(select [Room].ID from [Room] inner join [Booking] on [Room].ID = [Booking].RoomID "
                    + "where @checkInS < [Booking].CheckOut and [Booking].CheckIn < @checkOutS)", roId, checkIn, checkOut)
                .Select(ro => new Room { ID = ro.ID, Level = ro.Level });

            var Rooms = await roomsAvailable.ToListAsync();


            if (ModelState.IsValid && Rooms.Count != 0 && checkDates)
            {
                var room = await _context.Room.SingleOrDefaultAsync(m => m.ID == booking.RoomID);
                // calculate the total cost of booking
                TimeSpan bookingDays = booking.CheckOut - booking.CheckIn;
                decimal bookedDays = (decimal)bookingDays.TotalDays;
                booking.Cost = room.Price * bookedDays;

                _context.Add(booking);
                await _context.SaveChangesAsync();

                ViewBag.RoomFloor = room.Level;

            }
            if (!checkDates)
            {
                ViewBag.DateError = "Check out date cannot be less than or the same as check in date";
            }
            else if (Rooms.Count == 0)
            {
                ViewBag.Error = "This room is not available for your selected time period";
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "ID", booking.RoomID);
            return View(booking);
        }
    }
}

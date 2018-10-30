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

namespace WebHotel.Controllers
{
    [Authorize(Roles = "Customers")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index (string sortOrder)
        {
            if (String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "cost_asc";
            }
            var bookings = (IQueryable<Booking>)_context.Booking.Include(b => b.TheCustomer).Include(b => b.TheRoom).Where(b => b.CustomerEmail == User.Identity.Name);
            // Sort the Booking by specified order
            switch (sortOrder)
            {
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
        public IActionResult Create()
        {
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email");
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "Level");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            if (ModelState.IsValid)
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "Level", booking.RoomID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
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
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "Level", booking.RoomID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            if (id != booking.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            ViewData["RoomID"] = new SelectList(_context.Set<Room>(), "ID", "Level", booking.RoomID);
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.ID == id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.ID == id);
        }
    }
}

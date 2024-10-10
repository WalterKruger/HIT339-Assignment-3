using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A3_G4.Data;
using A3_G4.Models;

namespace A3_G4.Controllers
{
    public class EventCoachesController : Controller
    {
        private readonly A3Context _context;
        private readonly Hitdb1Context _hitdb1;

        public EventCoachesController(A3Context context, Hitdb1Context hitdb1Context)
        {
            _context = context;
            _hitdb1 = hitdb1Context;
        }

        private bool EventCoachExists(int id)
        {
            return _context.EventCoach.Any(e => e.Id == id);
        }

        // ALWAYS CHECK THIS WHEN ADDING OR MODIFIYING A ENTRY
        private bool VerifyForeignKeys(EventCoach toVerify)
        {
            return
                _hitdb1.Coaches.Any(i => i.CoachId == toVerify.CoachId) &&
                _hitdb1.Schedules.Any(i => i.ScheduleId == toVerify.ScheduleId);
        }


        // GET: EventCoaches
        public async Task<IActionResult> Index()
        {
            return View(await _context.EventCoach.ToListAsync());
        }

        // GET: EventCoaches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventCoach = await _context.EventCoach
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventCoach == null)
            {
                return NotFound();
            }

            return View(eventCoach);
        }

        // GET: EventCoaches/Create
        public IActionResult Create()
        {
            // Custom dropdown list
            var memberSelect = _hitdb1.Coaches.Select(i => new { i.CoachId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["CoachFK"] = new SelectList(memberSelect, "CoachId", "FullName");

            var scheduleSelect = _hitdb1.Schedules.Select(i => new { i.ScheduleId, i.Name }).ToList();
            ViewData["ScheduleFK"] = new SelectList(scheduleSelect, "ScheduleId", "Name");

            return View();
        }

        // POST: EventCoaches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CoachId,ScheduleId")] EventCoach eventCoach)
        {
            if (ModelState.IsValid && VerifyForeignKeys(eventCoach))
            {
                _context.Add(eventCoach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }

        // GET: EventCoaches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventCoach = await _context.EventCoach.FindAsync(id);
            if (eventCoach == null)
            {
                return NotFound();
            }

            // Custom dropdown list
            var memberSelect = _hitdb1.Coaches.Select(i => new { i.CoachId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["CoachFK"] = new SelectList(memberSelect, "CoachId", "FullName");

            var scheduleSelect = _hitdb1.Schedules.Select(i => new { i.ScheduleId, i.Name }).ToList();
            ViewData["ScheduleFK"] = new SelectList(scheduleSelect, "ScheduleId", "Name");

            return View(eventCoach);
        }

        // POST: EventCoaches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CoachId,ScheduleId")] EventCoach eventCoach)
        {
            if (id != eventCoach.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid && VerifyForeignKeys(eventCoach))
            {
                try
                {
                    _context.Update(eventCoach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventCoachExists(eventCoach.Id))
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
            return View(eventCoach);
        }

        // GET: EventCoaches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventCoach = await _context.EventCoach
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventCoach == null)
            {
                return NotFound();
            }

            return View(eventCoach);
        }

        // POST: EventCoaches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventCoach = await _context.EventCoach.FindAsync(id);
            if (eventCoach != null)
            {
                _context.EventCoach.Remove(eventCoach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

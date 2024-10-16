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
    public class EnrolledMembersController : Controller
    {
        private readonly A3Context _context;
        private readonly Hitdb1Context _hitdb1;

        public EnrolledMembersController(A3Context context, Hitdb1Context hitdb1Context)
        {
            _context = context;
            _hitdb1 = hitdb1Context;
        }

        // ALWAYS CHECK THIS WHEN ADDING OR MODIFIYING A ENTRY
        private bool VerifyForeignKeys(EnrolledMember toVerify)
        {
            return 
                _hitdb1.Members.Any(i => i.MemberId == toVerify.MemberId) &&
                _hitdb1.Schedules.Any(i => i.ScheduleId == toVerify.ScheduleId);
        }

        private bool EnrolledMemberExists(int id)
        {
            return _context.EnrolledMember.Any(e => e.Id == id);
        }

        // GET: EnrolledMembers
        public async Task<IActionResult> Index()
        {
            return View(await _context.EnrolledMember.ToListAsync());
        }

        // GET: EnrolledMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrolledMember = await _context.EnrolledMember
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrolledMember == null)
            {
                return NotFound();
            }

            return View(enrolledMember);
        }

        // GET: EnrolledMembers/Create
        public IActionResult Create()
        {
            // Custom dropdown list
            var memberSelect = _hitdb1.Members.Select(i => new { i.MemberId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["MemberFK"] = new SelectList(memberSelect, "MemberId", "FullName");

            var scheduleSelect = _hitdb1.Schedules.Select(i => new { i.ScheduleId, i.Name }).ToList();
            ViewData["ScheduleFK"] = new SelectList(scheduleSelect, "ScheduleId", "Name");

            return View();
        }

        // POST: EnrolledMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberId,ScheduleId")] EnrolledMember enrolledMember)
        {
            if (ModelState.IsValid && VerifyForeignKeys(enrolledMember))
            {
                _context.Add(enrolledMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }

        // GET: EnrolledMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrolledMember = await _context.EnrolledMember.FindAsync(id);
            if (enrolledMember == null)
            {
                return NotFound();
            }

            // Custom dropdown list
            var memberSelect = _hitdb1.Members.Select(i => new { i.MemberId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["MemberFK"] = new SelectList(memberSelect, "MemberId", "FullName");

            var scheduleSelect = _hitdb1.Schedules.Select(i => new { i.ScheduleId, i.Name }).ToList();
            ViewData["ScheduleFK"] = new SelectList(scheduleSelect, "ScheduleId", "Name");

            return View(enrolledMember);
        }

        // POST: EnrolledMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,ScheduleId")] EnrolledMember enrolledMember)
        {
            if (id != enrolledMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid && VerifyForeignKeys(enrolledMember))
            {
                try
                {
                    _context.Update(enrolledMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrolledMemberExists(enrolledMember.Id))
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
            return View(enrolledMember);
        }

        // GET: EnrolledMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrolledMember = await _context.EnrolledMember
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrolledMember == null)
            {
                return NotFound();
            }

            return View(enrolledMember);
        }

        // POST: EnrolledMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrolledMember = await _context.EnrolledMember.FindAsync(id);
            if (enrolledMember != null)
            {
                _context.EnrolledMember.Remove(enrolledMember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // TODO: Change this to get the currently logged in user and redirect if not logged in
        public async Task<IActionResult> Enrol()
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null) { return NotFound(); }
            int id = Int32.Parse(userIdClaim.Value);

            var currentlyEnrolled = _context.EnrolledMember
                .Where(m => m.MemberId == id)
                .Select(i => i.ScheduleId)
                .ToHashSet();

            var scheduleSelect = _hitdb1.Schedules
                .Where(i => !currentlyEnrolled.Contains(i.ScheduleId))
                .Select(i => new { i.ScheduleId, i.Name }).ToListAsync();
            ViewData["ScheduleFK"] = new SelectList(await scheduleSelect, "ScheduleId", "Name");

            return View();
        }

        // POST: EnrolledMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Enrol")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrolConfirmed([Bind("Id,MemberId,ScheduleId")] EnrolledMember enrolledMember)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null) { return NotFound(); }
            enrolledMember.MemberId = Int32.Parse(userIdClaim.Value);

            bool alreadyEnrolled = _context.EnrolledMember
                .Where(i => (i.ScheduleId == enrolledMember.ScheduleId) && (i.MemberId == enrolledMember.MemberId))
                .Any();

            if (ModelState.IsValid && VerifyForeignKeys(enrolledMember) && !alreadyEnrolled)
                {
                _context.Add(enrolledMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MySchedule));
            }
            return RedirectToAction(nameof(Enrol));
        }



        public async Task<IActionResult> MySchedule() {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null) { return NotFound(); }
            int id = Int32.Parse(userIdClaim.Value);

            bool memberExists = _hitdb1.Members.Any(i => i.MemberId == id);
            if (!memberExists) { return NotFound(); }

            var scheduleFK = _context.EnrolledMember
                .Where(i => i.MemberId == id)
                .Select(i => i.ScheduleId)
                .ToHashSet();

            var coachNameFromID = _hitdb1.Coaches.ToDictionary(i => i.CoachId, i => $"{i.FirstName} {i.LastName}");

            var coachesFK = _context.EventCoach
                .GroupBy(ec => ec.ScheduleId)
                .ToDictionary(
                    i => i.Key, 
                    i => i.Select(ec => new { Id = ec.CoachId, Name = coachNameFromID[ec.CoachId] } ).ToList()
            );

            var myEvents = _hitdb1.Schedules
                .Where(i => scheduleFK.Contains(i.ScheduleId))
                .ToList();

            // TODO: Each Dic entry needs to contain both a full name and a Id
            // (So it can be used to "View coach profiles from a list, and from the schedule details")
            ViewBag.coachesFK = coachesFK;

            return View(myEvents);
        }
        
    }
}

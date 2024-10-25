using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A3_G4.Data;
using A3_G4.Models;
using Microsoft.AspNetCore.Authorization;

namespace A3_G4.Controllers
{
    public class CoachesBioController : Controller
    {
        private readonly Hitdb1Context _context;
        private readonly A3Context _localdb;

        public CoachesBioController(Hitdb1Context context, A3Context localcontext)
        {
            _context = context;
            _localdb = localcontext;
        }

        // GET: CoachesBio
        /*       public async Task<IActionResult> Index()
               {
                   return View(await _context.Coaches.ToListAsync());
               }*/
        public IActionResult Index()
        {
            List<Coach> coaches = _context.Coaches.ToList();
            List<CoachBio> coachBios = coaches.Select(c => new CoachBio
            {
                // Mapping logic here, for example:
                CoachId = c.CoachId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Biography = c.Biography,
                Photo = c.Photo,

    }).ToList();

            return View(coachBios);
        }


        public class CoachScheduleViewModel
        {
            public Schedule Schedule { get; set; }
            public List<Member> Members { get; set; }
        }


        // GET: CoachesBio/CoachBioSchedule/5
        public async Task<IActionResult> CoachBioSchedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleCoachFK = _localdb.EventCoach
                .Where(i => i.CoachId == id)
                .Select(i => i.ScheduleId)
                .ToHashSet();

            var scheduleForCoach = await _context.Schedules
                .Where(i => scheduleCoachFK.Contains(i.ScheduleId))
                .ToListAsync();


            var memberNameFromID = _context.Members.ToDictionary(i => i.MemberId, i => $"{i.FirstName} {i.LastName}");

            var memberIdForSchedule = _localdb.EnrolledMember
                //.Where(i => scheduleCoachFK.Contains(i.ScheduleId))
                .GroupBy(i => i.ScheduleId)
                .ToDictionary(
                    i => i.Key,
                    i => i.Select(m => new { Id = m.MemberId, Name = memberNameFromID[m.MemberId] })
                );

            ViewBag.membersFK = memberIdForSchedule;


            return View(scheduleForCoach);
        }

        // Redirect coach to their `CoachBioSchedule` page based on their login information
        public IActionResult MyScheduleCoach() {
            var MemberIdClaim = User.FindFirst("CoachId");

            if (MemberIdClaim == null) { return NotFound(); }
            int id = Int32.Parse(MemberIdClaim.Value);

            return RedirectToAction("CoachBioSchedule", new { id });
        }


        // GET: CoachesBio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachBio = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coachBio == null)
            {
                return NotFound();
            }

            return View(coachBio);
        }

        // GET: CoachesBio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CoachesBio/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoachId,FirstName,LastName,Biography,Photo")] CoachBio coachBio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coachBio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coachBio);
        }




        // GET: Coaches/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches.FindAsync(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }

        // POST: Coaches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CoachId,FirstName,LastName,Biography,Photo")] Coach coach)
        {
            if (id != coach.CoachId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoachExists(coach.CoachId))
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
            return View(coach);
        }






        /*  // GET: CoachesBio/Edit/5
          public async Task<IActionResult> Edit(int? id)
          {
              if (id == null)
              {
                  return NotFound();
              }

              var coachBio = await _context.Coaches.FindAsync(id);
              if (coachBio == null)
              {
                  return NotFound();
              }
              return View(coachBio);
          }






          // POST: CoachesBio/Edit/5
          // To protect from overposting attacks, enable the specific properties you want to bind to.
          // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(int id, [Bind("CoachId,FirstName,LastName,Biography,Photo")] CoachBio coachBio)
          {
              if (id != coachBio.CoachId)
              {
                  return NotFound();
              }

              if (ModelState.IsValid)
              {
                  try
                  {
                      _context.Update(coachBio);
                      await _context.SaveChangesAsync();
                  }
                  catch (DbUpdateConcurrencyException)
                  {
                      if (!CoachExists(coachBio.CoachId))
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
              return View(coachBio);
          }*/

        // GET: CoachesBio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachBio = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coachBio == null)
            {
                return NotFound();
            }

            return View(coachBio);
        }

        // POST: CoachesBio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coachBio = await _context.Coaches.FindAsync(id);
            if (coachBio != null)
            {
                _context.Coaches.Remove(coachBio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoachExists(int id)
        {
            return _context.Coaches.Any(e => e.CoachId == id);
        }
    }
}

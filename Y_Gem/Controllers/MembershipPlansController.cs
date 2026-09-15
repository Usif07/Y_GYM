using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Y_Gem.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class MembershipPlansController : Controller
    {
        IMembershipPlans memPlanRepo;
        public MembershipPlansController(IMembershipPlans memPlanRepo)
        {
            this.memPlanRepo = memPlanRepo;
        }




        public IActionResult Index()
        {
            var MembershipPlans = memPlanRepo.GetAll();
            return View(MembershipPlans);
        }




        public IActionResult Delete(int id)
        {
            memPlanRepo.delete(id);
            memPlanRepo.save();
            return RedirectToAction("Index");

        }


        //public IActionResult Delete(int id)
        //{
        //    var mp = memPlanRepo.GetById(id);   

        //    if (mp != null)
        //    {
        //        memPlanRepo.(mp);
        //        memPlanRepo.save();
        //    }

        //    return RedirectToAction("Index");
        //}
        [HttpGet]
        public IActionResult Update(int id)
        {
         var plan = memPlanRepo.GetById(id);    
            return View(plan);
        }
        [HttpPost]
        public IActionResult Update(MembershipPlan mp)
        {
            if (ModelState.IsValid)
            {
                memPlanRepo.update(mp);
                memPlanRepo.save();
                return RedirectToAction("Index");
            }
            else { return View(mp); }
        }
                
     
        public IActionResult New()
        {
            return View("New");
        }

        [HttpPost]
        public IActionResult SaveNew(MembershipPlan mp)
        {
            if (ModelState.IsValid)
            {
                memPlanRepo.insert(mp);
                memPlanRepo.save();
                return RedirectToAction("Index");
            }
            return View("New", mp);
        }
       
    }
}

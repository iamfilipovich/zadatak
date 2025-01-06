using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wiener.Models;
using Wiener.Models.DTO;
using Wiener.Repositories.Abstract;

namespace Wiener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPartnerService _partnerService;
        private readonly IPolicyService _policyService;

        public HomeController(ILogger<HomeController> logger, IPartnerService partnerService,
                              IPolicyService policyService)
        {
            _logger = logger;
            _partnerService = partnerService;
            _policyService = policyService;
        }

        public async Task<IActionResult> Index()
        {
            var partners = await _partnerService.GetAllPartnersAsync();
            return View(partners);
        }

        [HttpGet]
        public IActionResult AddPartner()
        {
            return View(new Partner());
        }

        [HttpPost]
        public async Task<IActionResult> AddPartner(Partner partner)
        {
            if (_partnerService == null)
            {
                throw new InvalidOperationException("_partnerService is null");
            }

            if (await _partnerService.PartnerExistsAsync(partner.PartnerNumber))
            {
                ModelState.AddModelError("PartnerNumber", "Partner with the same PartnerNumber already exists.");
            }

            if (await _partnerService.ExternalCodeExistsAsync(partner.ExternalCode))
            {
                ModelState.AddModelError("ExternalCode", "Partner with the same ExternalCode already exists.");
            }

            if (ModelState.IsValid)
            {
                await _partnerService.AddPartner(partner);
                return RedirectToAction("Index");
            }

            return View(partner);
        }

        [HttpGet]
        public async Task<JsonResult> GetPartnerDetails(int id)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(id);
            if (partner == null)
            {
                return Json(new { success = false, message = "Partner not found." });
            }

            return Json(new
            {
                success = true,
                address = partner.Address,
                createdByUser = partner.CreateByUser,
                externalCode = partner.ExternalCode,
                createdAt = partner.CreatedAtUtc
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePartner(string partnerNumber)
        {
            bool isDeleted = await _partnerService.DeletePartnerAsync(partnerNumber);

            if (isDeleted)
            {
                return RedirectToAction("Index");  
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Partner not found or failed to delete.");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPartner(int id)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(id);
            if (partner == null)
            {
                return RedirectToAction("Index");
            }

            return View(partner);
        }

        [HttpPost]
        public async Task<IActionResult> EditPartner(Partner partner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var updated = await _partnerService.UpdatePartnerAsync(partner);
                    if (updated)
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "External code already exists.");
                }
            }

            return View(partner);
        }


        [HttpGet]
        public async Task<IActionResult> AddPolicy(int id)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(id);

            if (partner == null)
            {
                TempData["ErrorMessage"] = "Partner not found.";
                return RedirectToAction("Index");
            }

            var viewModel = new AddPolicyViewModel
            {
                Partner = partner,
                Policy = new Policy { PartnerId = partner.Id }
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddPolicy(AddPolicyViewModel model)
        {
            if (model.Policy == null)
            {
                ModelState.AddModelError(string.Empty, "Policy data is missing.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _policyService.AddPolicy(model.Policy);
            await _policyService.UpdatePartnerStatus(model.Policy.PartnerId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Failed to add policy.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Policy added successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost("mark-partner-as-old/{partnerId}")]
        public async Task<IActionResult> MarkPartnerAsOld(int partnerId)
        {
            try
            {
                await _partnerService.MarkPartnerAsOldAsync(partnerId);
                return Ok(new { message = "Partner marked as old successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

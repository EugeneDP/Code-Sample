using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class DeploymentController : DeployementControllerBase
    {
        public DeploymentController(IRepository<DeploymentJob> repository)
            : base(repository)
        {
        }

        public DeploymentController()
            : this(new MongoRepository<DeploymentJob>())
        {

        }
        public ActionResult Index()
        {
            var model = new DeploymentJointModel();
            var Companies = new MongoRepository<Company>();
            model.ModelForView.Company = Companies.Select(c => new SelectListItem
                {
                    Value = c.CompanyKey,
                    Text = c.CompanyName
                });

            var Environments = new MongoRepository<Entities.Environment>();
            model.ModelForView.Environment = Environments.Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = e.Name
                });

            var Revisions = new MongoRepository<Revision>();
            model.ModelForView.Revision = Revisions.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Tag
                });

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(DeployDataFromView model)
        {
            var deploy = new DeploymentJob();
            deploy.Id = Guid.NewGuid().ToString();
            deploy.Date = DateTime.Now;

            var Companies = new MongoRepository<Company>();
            deploy.Company = Companies.Single(c => c.CompanyKey == model.deployData["Company"]);
            var Environments = new MongoRepository<Entities.Environment>();
            deploy.Server = Environments.Single(e=>e.Id==model.deployData["Environment"]);
            var Revisions = new MongoRepository<Revision>();
            deploy.Revision = Revisions.Single(r=>r.Id== model.deployData["Revision"]);
            if (model.deployData["DeploySide"] == "Server")
            {
                if (model.deployData.ContainsKey("Database") && model.deployData["Database"]=="True")
                {
                    deploy.Database = true;
                }
                if (model.deployData.ContainsKey("Create") && model.deployData["Create"] == "True")
                {
                    deploy.Create = true;
                }
            }
            else
            {
                deploy.ServerSide = false;
                deploy.ClientSide = true;
                if (model.deployData.ContainsKey("Android") && model.deployData["Android"] == "True")
                {
                    deploy.Android = true;
                }
                if (model.deployData.ContainsKey("Callbox") && model.deployData["Callbox"] == "True")
                {
                    deploy.CallBox = true;
                }
                if (model.deployData.ContainsKey("IosAdhoc") && model.deployData["IosAdhoc"] == "True")
                {
                    deploy.IosAdhoc = true;
                }
                if (model.deployData.ContainsKey("IosAppStore") && model.deployData["IosAppStore"] == "True")
                {
                    deploy.IosAppStore = true;
                }
            }

            Repository.Add(deploy);
            return View();
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Areas.Admin.Repository;
using CustomerPortal.Web.Entities;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{

    [Authorize(Roles = RoleName.Admin)]
    public class RevisionController : RevisionControllerBase
    {
        public RevisionController(IRepository<Revision> repository)
            : base(repository)
        {
        }

        public RevisionController()
            : this(new MongoRepository<Revision>())
        {

        }
        public ActionResult Index()
        {
            var repository = new MongoRepository<Revision>();
            if (!repository.Any())
            {
                return RedirectToAction("Update","Revision");
            }
            return View(repository);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Revision revision)
        {
            revision.Id = Guid.NewGuid().ToString();
            Repository.Add(revision);
            return View();
        }
   
        public ActionResult Update()
        {
            var req = WebRequest.Create("https://bitbucket.org/api/1.0/repositories/mk-cab/tags") as HttpWebRequest;
            var authInfo = "buildcab:admin200!";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
            string result = null;
            try
            {
                using (var resp = req.GetResponse() as HttpWebResponse)
                {
                    var reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch
            {
                //return new Dictionary<string, BitbucketTagsResponse>();
            }

            var revisionsFromBitBucket   = JsonConvert.DeserializeObject<Dictionary<string, BitbucketTagsResponse>>(result).Select( x => new Revision
                {
                    Id = Guid.NewGuid().ToString(),
                    Commit = x.Value.node,
                    Tag = x.Key
                }).ToList();

            
            //existing revisions => update
            var updatesRevisions = (from rev in Repository.ToList()
                                    let revBitbucket = revisionsFromBitBucket.FirstOrDefault(x => x.Tag == rev.Tag)
                                    where revBitbucket != null && revBitbucket.Commit != rev.Commit
                                    select new Revision
                                        {
                                            Id = rev.Id,
                                            Tag = rev.Tag,
                                            Commit = revBitbucket.Commit
                                        }).ToList();

            if (updatesRevisions.Any())
            {
                Repository.Update(updatesRevisions);
            }

            var tagsFromRepository = Repository.Select(x => x.Tag).ToList();
            //new revisions in bitbucket => add to repository
            var newRevisions = revisionsFromBitBucket.Where(x => !tagsFromRepository.Contains(x.Tag)).ToList();
            if(newRevisions.Any())
            {
                Repository.Add(newRevisions);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(ChangeRevisionStatus model)
        {
            var revision = Repository.GetById(model.status[0]);
            if (revision == null) return HttpNotFound();
            switch (model.status[1])
            {
                case("true"):
                    revision.Hidden = true;
                    break;
                default:
                    revision.Hidden = false;
                    break;
            }
            Repository.Update(revision);
            return RedirectToAction("Index");
        }
    }
}

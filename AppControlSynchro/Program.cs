/////////////////////////////////////////////////////////////////////////////////////////////////
//	This tool imports a list of people from a CSV file into the AppControl People table
//
//	Structure of the CSV file (no header line, ';' separator):
//
//	FirstName;LastName;PersonID;Department;ValidDateFrom;ValidDateTo;DateOfBirth
/////////////////////////////////////////////////////////////////////////////////////////////////

using SynchroAppControl.AppControlServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SynchroAppControl
{
    class Program
    {
        // AppVision server and account (test bench defaults). Use https and a dedicated account on a real site.
        const string ServiceUrl = "http://localhost/AppControlDataService.svc/";
        const string UserName = "ADM";
        const string Password = "123";

        static void Main(string[] args)
        {
            if (args?.Length < 1)
            {
                Console.WriteLine("Usage: SynchroAppControl csv_filename");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Trace($"{args[0]} doesn't exist.");
                return;
            }

            try
            {
                Trace("Start SynchroAppControl");
                var ctx = new AppControlContext(new Uri(ServiceUrl))
                {
                    Credentials = new NetworkCredential(UserName, Password),
                    // The proxy classes were generated from an older server: ignore the properties added since then
                    // instead of failing with "The property 'X' does not exist on type ...".
                    IgnoreMissingProperties = true,
                };
                int count = 0;
                foreach (var l in File.ReadAllLines(args[0]))
                    if (ImportLine(ctx, l))
                        count++;
                // sends the queued additions and updates, one HTTP request per person
                ctx.SaveChanges();
                Trace($"{count} people added or updated.");
            }
            catch (Exception ex)
            {
                // the server message (401, duplicate key...) is in the inner exception
                Trace($"Exception {ex.Message} {ex.InnerException?.Message}");
            }
            finally
            {
                Trace("End SynchroAppControl");
            }
        }

        static int? DEFAULT_CATEGORY_ID = 1;
        private static bool ImportLine(AppControlContext ctx, string l)
        {
            var ss = l.Split(';');
            if (ss.Length == 7)
            {
                var dept = ctx.Departments.Where(a => a.Description == ss[3]).FirstOrDefault();
                DateTime validityStart, validityEnd;
                if (string.IsNullOrEmpty(ss[4]))
                    validityStart = DateTime.Now.Date;
                else if (!DateTime.TryParse(ss[4], out validityStart))
                {
                    Trace($"Validity start invalid: {ss[4]}");
                    return false;
                }
                if (string.IsNullOrEmpty(ss[5]))
                    validityEnd = validityStart.AddYears(1);
                else if (!DateTime.TryParse(ss[5], out validityEnd))
                {
                    Trace($"Validity end invalid: {ss[5]}");
                    return false;
                }

                Person pNew = new Person { FirstName = ss[0], LastName = ss[1], CompanyId = ss[2], DepartmentId = dept?.Id, ValidityStart = validityStart, ValidityEnd = validityEnd, IsActive = true, CategoryId = DEFAULT_CATEGORY_ID, Info1 = ss[6], DateCreation = DateTime.Now, DateModification = DateTime.Now };

                // check person
                Person pOld = ctx.People.Where(p => p.CompanyId == pNew.CompanyId).FirstOrDefault();
                if (pOld == null)
                {
                    Trace($" add {pNew.CompanyId} {pNew.FirstName} {pNew.LastName}");
                    ctx.AddToPeople(pNew);
                }
                else if (pOld.FirstName == pNew.FirstName && pOld.LastName == pNew.LastName && pOld.DepartmentId == pNew.DepartmentId && pOld.ValidityStart == pNew.ValidityStart && pOld.ValidityEnd == pNew.ValidityEnd && pOld.Info1 == pNew.Info1)
                {
                    Trace($" no change {pNew.CompanyId} {pNew.FirstName} {pNew.LastName}");
                    return false;
                }
                else
                {
                    Trace($" update {pNew.CompanyId} {pNew.FirstName} {pNew.LastName}");
                    pOld.FirstName = pNew.FirstName;
                    pOld.LastName = pNew.LastName;
                    pOld.DepartmentId = pNew.DepartmentId;
                    pOld.ValidityStart = pNew.ValidityStart;
                    pOld.ValidityEnd = pNew.ValidityEnd;
                    pOld.Info1 = pNew.Info1;
                    pOld.DateModification = DateTime.Now;
                    ctx.UpdateObject(pOld);
                }
                return true;
            }
            else
                Trace("Incorrect line: " + l);
            return false;
        }

        static void Trace(string text)
        {
            Console.WriteLine(text);
            File.AppendAllText("SynchroAppControl.log", DateTime.Now + " " + text + "\r\n");
        }
    }
}

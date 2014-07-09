using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace GroupHug.Website.Services {

    public class ActiveDirectoryServer {

        //public IEnumerable<Employee> ListEmployees() {
        //    employees = new List<Employee>();

        //    using (principalSearcher = new PrincipalSearcher(new UserPrincipal(principalContext))) {
        //        foreach (var searchResult in principalSearcher.FindAll()) {
        //            var directoryEntry = searchResult.GetUnderlyingObject() as DirectoryEntry;

        //            // We use the main phone number field in AD to track current real human staff vs. everything else
        //            if (String.IsNullOrEmpty(directoryEntry.Properties["telephoneNumber"].Value as string))
        //                continue;

        //            // Occasionally search returns the same person twice - so we safeguard against that here.
        //            if (
        //                employees.Any(
        //                    e => e.samAccountName == directoryEntry.Properties["sAMAccountname"].Value as string))
        //                continue;

        //            employees.Add(new Employee() {
        //                DisplayName = directoryEntry.Properties["displayName"].Value as string,
        //                PrincipalName = directoryEntry.Properties["userPrincipalname"].Value as string,
        //                samAccountName = directoryEntry.Properties["sAMAccountname"].Value as string,
        //                JobTitle = directoryEntry.Properties["title"].Value as string,
        //                FirstName = directoryEntry.Properties["givenName"].Value as string,
        //                LastName = directoryEntry.Properties["sn"].Value as string,
        //                Department = directoryEntry.Properties["department"].Value as string,
        //                Description = directoryEntry.Properties["description"].Value as string,
        //                Telephone = directoryEntry.Properties["telephoneNumber"].Value as string,
        //                DirectDial = directoryEntry.Properties["otherTelephone"].Value as string,
        //                EmailAddress = directoryEntry.Properties["mail"].Value as string,
        //                Manager = directoryEntry.Properties["manager"].Value as string
        //                //GroupMemberships = new List<GroupMembership>().Add(new GroupMembership() { GroupName = Convert.ToString(directoryEntry.Properties["memberOf"].Value) }),
        //            });

        //        }
        //    }

        //    return employees;
        //}

        public Employee Find(IIdentity identity) {
            return (FindByName(identity.Name));
        }

        public Employee FindByName(string name) {

            var principalContext = new PrincipalContext(ContextType.Domain);
            var user = UserPrincipal.FindByIdentity(principalContext, name);
            var employee = new Employee() {
                SamAccountName = user.SamAccountName,
                DisplayName = user.DisplayName,
                PrincipalName = user.UserPrincipalName,
                EmailAddress = user.EmailAddress
            };

            //DisplayName = directoryEntry.Properties["displayName"].Value as string,
            //PrincipalName = directoryEntry.Properties["userPrincipalname"].Value as string,
            //JobTitle = directoryEntry.Properties["title"].Value as string,
            //FirstName = directoryEntry.Properties["givenName"].Value as string,
            //LastName = directoryEntry.Properties["sn"].Value as string,
            //Department = directoryEntry.Properties["department"].Value as string,
            //Description = directoryEntry.Properties["description"].Value as string,
            //Telephone = directoryEntry.Properties["telephoneNumber"].Value as string,
            //DirectDial = directoryEntry.Properties["otherTelephone"].Value as string,
            //EmailAddress = directoryEntry.Properties["mail"].Value as string,
            ////GroupMemberships = new List<GroupMembership>().Add(new GroupMembership() { GroupName = Convert.ToString(directoryEntry.Properties["memberOf"].Value) }),
            return employee;
        }
    }

    public class Employee : IIdentity {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string PrincipalName { get; set; }
        public string SamAccountName { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public string EmailAddress { get; set; }
        public string Telephone { get; set; }
        public string DirectDial { get; set; }
        public string TelephoneNumber3 { get; set; }
        public string Manager { get; set; }

        public string Name {
            get { return (DisplayName); }
        }

        public string AuthenticationType {
            get { return ("Employee"); }
        }

        public bool IsAuthenticated {
            get { return (true); }
        }

        public object ImageUrl { get; set; }
    }
}
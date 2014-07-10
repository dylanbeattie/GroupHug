using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace GroupHug.Website.Services {

    public class ActiveDirectoryServer {

        public IEnumerable<Employee> ListEmployees() {
            var employees = new List<Employee>();
            using (var principalContext = new PrincipalContext(ContextType.Domain)) {
                using (var principalSearcher = new PrincipalSearcher(new UserPrincipal(principalContext))) {
                    foreach (var searchResult in principalSearcher.FindAll()) {
                        var directoryEntry = searchResult.GetUnderlyingObject() as DirectoryEntry;

                        // We use the main phone number field in AD to track current real human staff vs. everything else
                        if (directoryEntry.Properties["photo"].Value == null) continue;

                        // Occasionally search returns the same person twice - so we safeguard against that here.
                        if (
                            employees.Any(
                                e => e.SamAccountName == directoryEntry.Properties["sAMAccountname"].Value as string))
                            continue;
                        var employee = new Employee() {
                            DisplayName = directoryEntry.Properties["displayName"].Value as string,
                            PrincipalName = directoryEntry.Properties["userPrincipalname"].Value as string,
                            SamAccountName = directoryEntry.Properties["sAMAccountname"].Value as string,
                            JobTitle = directoryEntry.Properties["title"].Value as string,
                            FirstName = directoryEntry.Properties["givenName"].Value as string,
                            LastName = directoryEntry.Properties["sn"].Value as string,
                            Department = directoryEntry.Properties["department"].Value as string,
                            Description = directoryEntry.Properties["description"].Value as string,
                            Telephone = directoryEntry.Properties["telephoneNumber"].Value as string,
                            DirectDial = directoryEntry.Properties["otherTelephone"].Value as string,
                            EmailAddress = directoryEntry.Properties["mail"].Value as string,
                            Manager = directoryEntry.Properties["manager"].Value as string
                            //GroupMemberships = new List<GroupMembership>().Add(new GroupMembership() { GroupName = Convert.ToString(directoryEntry.Properties["memberOf"].Value) }),
                        };
                        var photo = directoryEntry.Properties["photo"].Value as byte[];
                        if (photo != null) employee.Photo = System.Text.Encoding.UTF8.GetString(photo);
                        employees.Add(employee);
                    }
                }
            }

            return employees;
        }

        public Employee Find(IIdentity identity) {
            return (FindByName(identity.Name));
        }

        public Employee FindByName(string name) {

            using (var principalContext = new PrincipalContext(ContextType.Domain)) {
                using (var user = UserPrincipal.FindByIdentity(principalContext, name)) {
                    if (user == null) return (null);
                    var employee = new Employee() {
                        SamAccountName = user.SamAccountName,
                        DisplayName = user.DisplayName,
                        PrincipalName = user.UserPrincipalName,
                        EmailAddress = user.EmailAddress,
                    };
                    var photo = ((DirectoryEntry)user.GetUnderlyingObject()).Properties["photo"].Value as byte[];
                    if (photo != null) employee.Photo = System.Text.Encoding.UTF8.GetString(photo);

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
        public string Photo { get; set; }

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
        public string JobTitle { get; set; }
    }
}
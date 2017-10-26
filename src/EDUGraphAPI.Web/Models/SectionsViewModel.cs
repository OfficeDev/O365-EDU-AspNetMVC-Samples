/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;
using System.Linq;
using Microsoft.Education;
using EDUGraphAPI.Web.Models;

namespace EDUGraphAPI.Web.ViewModels
{
    public class SectionsViewModel
    {
        public SectionsViewModel(string userEmail, EducationSchool school, ArrayResult<EducationClass> classes, IEnumerable<EducationClass> myClasses)
        {
            this.UserEmail = userEmail;
            this.School = school;
            this.Classes = classes;
            this.MyClasses = myClasses.ToList();
        }

        public SectionsViewModel(UserContext userContext, EducationSchool school, ArrayResult<EducationClass> classes, IEnumerable<EducationClass> myClasses)
        {
            this.UserEmail = userContext.UserO365Email;
            this.School = school;
            this.Classes = classes;
            this.MyClasses = myClasses.ToList();
            this.UserContext = userContext;
        }

        public string UserEmail { get; set; }

        public EducationSchool School { get; set; }

        public ArrayResult<EducationClass> Classes { get; set; }

        public List<EducationClass> MyClasses { get; set; }

        public UserContext UserContext { get; set; }

        public bool IsMy(EducationClass @class)
        {
            return MyClasses != null && MyClasses.Any(c => c.Id == @class.Id);
        }
    }
}
/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Education.Legacy;
using Microsoft.Education.Data.Legacy;

namespace EDUGraphAPI.Web.Models
{
    public class SchoolUsersViewModel
    {
        public SchoolUsersViewModel(School School, ArrayResult<SectionUser> users, ArrayResult<SectionUser> students, ArrayResult<SectionUser> teachers)
        {
            this.School = School;
            this.Users = users;
            this.Students = students;
            this.Teachers = teachers;
        }
        public SchoolUsersViewModel(UserContext userContext, School School, ArrayResult<SectionUser> users, ArrayResult<SectionUser> students, ArrayResult<SectionUser> teachers, ArrayResult<SectionUser> studentsInMyClasses)
        {
            this.School = School;
            this.Users = users;
            this.Students = students;
            this.Teachers = teachers;
            this.StudentsInMyClasses = studentsInMyClasses;
            this.UserContext = userContext;
        }
        public UserContext UserContext { get; set; }
        public School School { get; set; }
        public ArrayResult<SectionUser> Users { get; set; }
        public ArrayResult<SectionUser> Students { get; set; }
        public ArrayResult<SectionUser> Teachers { get; set; }
        public ArrayResult<SectionUser> StudentsInMyClasses { get; set; }
    }
}
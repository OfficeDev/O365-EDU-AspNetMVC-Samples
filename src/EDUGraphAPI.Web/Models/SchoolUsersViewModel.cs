/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Microsoft.Education;

namespace EDUGraphAPI.Web.Models
{
    public class SchoolUsersViewModel
    {
        public SchoolUsersViewModel(EducationSchool School, ArrayResult<EducationUser> users, ArrayResult<EducationUser> students, ArrayResult<EducationUser> teachers)
        {
            this.School = School;
            this.Users = users;
            this.Students = students;
            this.Teachers = teachers;
        }
        public SchoolUsersViewModel(UserContext userContext, EducationSchool School, ArrayResult<EducationUser> users, ArrayResult<EducationUser> students, ArrayResult<EducationUser> teachers, ArrayResult<EducationUser> studentsInMyClasses)
        {
            this.School = School;
            this.Users = users;
            this.Students = students;
            this.Teachers = teachers;
            this.StudentsInMyClasses = studentsInMyClasses;
            this.UserContext = userContext;
        }
        public UserContext UserContext { get; set; }
        public EducationSchool School { get; set; }
        public ArrayResult<EducationUser> Users { get; set; }
        public ArrayResult<EducationUser> Students { get; set; }
        public ArrayResult<EducationUser> Teachers { get; set; }
        public ArrayResult<EducationUser> StudentsInMyClasses { get; set; }
    }
}
/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;
using System.Linq;
using Microsoft.Education.Legacy;
using Microsoft.Education.Data.Legacy;
using EDUGraphAPI.Web.Models;

namespace EDUGraphAPI.Web.ViewModels
{
    public class SectionsViewModel
    {
        public SectionsViewModel(string userEmail, School School, ArrayResult<Section> sections, IEnumerable<Section> mySections)
        {
            this.UserEmail = userEmail;
            this.School = School;
            this.Sections = sections;
            this.MySections = mySections.ToList();
        }

        public SectionsViewModel(UserContext userContext, School School, ArrayResult<Section> sections, IEnumerable<Section> mySections)
        {
            this.UserEmail = userContext.UserO365Email;
            this.School = School;
            this.Sections = sections;
            this.MySections = mySections.ToList();
            this.UserContext = userContext;
        }

        public string UserEmail { get; set; }
        public School School { get; set; }
        public ArrayResult<Section> Sections { get; set; }
        public List<Section> MySections { get; set; }

        public UserContext UserContext { get; set; }
        public bool IsMy(Section section)
        {
            return MySections != null && MySections.Any(c => c.Email == section.Email);
        }
    }
}
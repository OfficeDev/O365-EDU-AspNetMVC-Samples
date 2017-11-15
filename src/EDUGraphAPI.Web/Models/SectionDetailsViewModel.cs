/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */


using Microsoft.Education;
using System.Collections.Generic;
using Microsoft.Education.Data;
using Microsoft.Graph;
using System.ComponentModel.DataAnnotations;
using System;

namespace EDUGraphAPI.Web.ViewModels
{
    public class SectionDetailsViewModel
    {
        public EducationSchool School { get; set; }

        public EducationClass Class { get; set; }
        
        public Conversation[] Conversations { get; set; }

        public string SeeMoreConversationsUrl { get; set; }

        public DriveItem[] DriveItems { get; set; }

        public string SeeMoreFilesUrl { get; set; }
        public bool IsStudent { get; set; }
        public string O365UserId { get; set; }
        public string MyFavoriteColor { get; set; }

        public SectionUser[] SchoolTeachers { get; set; }

        public Assignment[] Assignments { get; set; }


    }
}
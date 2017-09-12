/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Microsoft.Education.Data.Legacy;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.ViewModels
{
    public class SectionDetailsViewModel
    {
        public School School { get; set; }

        public Section Section { get; set; }
        
        public Conversation[] Conversations { get; set; }

        public string SeeMoreConversationsUrl { get; set; }

        public DriveItem[] DriveItems { get; set; }

        public string SeeMoreFilesUrl { get; set; }
        public bool IsStudent { get; set; }
        public string O365UserId { get; set; }
        public string MyFavoriteColor { get; set; }
    }
}
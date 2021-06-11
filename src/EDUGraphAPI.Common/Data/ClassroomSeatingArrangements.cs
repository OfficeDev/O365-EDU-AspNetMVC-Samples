/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.ComponentModel.DataAnnotations;

namespace EDUGraphAPI.Data
{
    public class ClassroomSeatingArrangements
    {
        [Key]
        public int Id { get; set; }

        public string O365UserId { get; set; }

        public int Position { get; set; }

        public string ClassId { get; set; }
    }
}

/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;

namespace EDUGraphAPI.Web.Models
{
    public class DemoPage
    {
        public string Controller { get; set; }

        public string Action { get; set; }

        public DemoPage()
        {
            this.Functions = new HashSet<Functions>();
        }

        public ISet<Functions> Functions { get; set; }
    }

    public class Functions
    {
        public Functions()
        {
            this.Files = new HashSet<Files>();
        }
        public string Title { get; set; }
        public string Tab { get; set; }
        public ISet<Files> Files { get; set; }
    }

    public class Files
    {
        public Files()
        {
            this.Methods = new HashSet<Method>();
        }
        public string Url { get; set; }



        public ISet<Method> Methods { get; set; }
    }

    public class Method
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
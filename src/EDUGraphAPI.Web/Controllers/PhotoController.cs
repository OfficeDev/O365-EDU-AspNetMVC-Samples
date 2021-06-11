/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Infrastructure;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.Controllers
{
    [EduAuthorize, HandleAdalException]
    public class PhotoController : Controller
    {
        //
        // GET: /Photo/UserPhoto/{Id of a user}
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public async Task<ActionResult> UserPhoto(string id)
        {
            var client = await AuthenticationHelper.GetGraphServiceClientAsync(Permissions.Application);
            var stream = await GetUserPhotoStreamAsync(client, id);
            if (stream != null) return File(stream, "image/jpeg");

            return File(Server.MapPath("/Content/Images/DefaultUserPhoto.jpg"), "image/jpeg");
        }

        private static async Task<Stream> GetUserPhotoStreamAsync(GraphServiceClient client, string id)
        {
            try
            {
                return await client.Users[id].Photo.Content.Request().GetAsync();
            }
            catch (ServiceException)
            {
                return null;
            }
        }
    }
}
/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
$(document).ready(function () {
    $("#userinfolink").click(function (evt) {
        evt.stopPropagation ? evt.stopPropagation() : evt.cancelBubble = true;
        $("#userinfoContainer").toggle();
        $("#caret").toggleClass("transformcaret");
    });
    $(document).click(function () {
        $("#userinfoContainer").hide();
        $("#caret").removeClass("transformcaret");

    });

    $('.demo-helper-control .header').on('click', function () {
        $(this).closest('.demo-helper-control').toggleClass('collapsed');
    });

    $('.message-container').fadeOut(5000);

    $(".demo-helper-control .body >ul> li >p").click(function (e) {
        if (e.target.tagName !== "A") {
            $(this).parent().toggleClass("collapse");
        }
    });
});

function showDemoHelper(tabName) {
    $("ul.functions > li").hide();
    $("ul.functions > li").each(function () {
        var text = $(this).find(".tab").text();
        if (text.indexOf(tabName) >= 0 || text === "") {
            $(this).show();
        }
    });
}
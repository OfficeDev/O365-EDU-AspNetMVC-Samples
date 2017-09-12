/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
$(document).ready(function () {
    loadImages();

    $(".teacher-student .filterlink-container .filterlink").click(function () {
        tabname = $(this).attr("id");
        showDemoHelper(tabname);
        search(true);
        var element = $(this);
        element.addClass("selected").siblings("a").removeClass("selected");
        var filterType = element.data("type");
        var tilesContainer = $(".teacher-student .tiles-root-container");
        tilesContainer.removeClass(tilesContainer.attr("class").replace("tiles-root-container", "")).addClass(filterType + "-container");
    });

    $(".teacher-student .tiles-root-container .pagination .prev, .teacher-student .tiles-root-container .pagination .next").click(function () {
        search(true);
        var element = $(this);
        if (element.hasClass("current") || element.hasClass("disabled")) {
            return;
        }

        var curPageElement = element.siblings("#curpage");
        var curPage = parseInt(curPageElement.val());
        var isBackward = element.hasClass("prev");
        var targetPageNum = curPage + (isBackward ? -1 : 1);
        var nextElement = isBackward ? element.siblings(".next") : element;
        var prevElement = isBackward ? element : element.siblings(".prev");
        var nextLinkElement = element.siblings("#nextlink");
        var nextLink = nextLinkElement.val();
        var hasNextLink = typeof (nextLink) == "string" && nextLink.length > 0;
        if (isBackward) {
            nextElement = element.siblings(".next");
            prevElement = element;
        }
        var container = element.closest(".tiles-secondary-container");
        var content = container.find(".content");
        var startItemNum = (targetPageNum - 1) * 12;
        if (startItemNum < content.children().length) {
            showPage(isBackward, targetPageNum, hasNextLink, prevElement, nextElement, curPageElement, content);
        }
        else if (hasNextLink) {
            var schoolId = $(".teacher-student input#school-objectid").val();
            var action = container.attr("id");
            action = action[0].toUpperCase() + action.substr(1);
            var url = "/Schools/" + schoolId + "/" + action + "/Next";

            var prevNext = prevElement.add(nextElement);
            prevNext.addClass("disabled");
            $.ajax({
                type: 'GET',
                url: url,
                dataType: 'json',
                data: { schoolId: schoolId, nextLink: nextLink },
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.error === "AdalException" || data.error === "Unauthorized") {
                        alert("Your current session has expired. Please click OK to refresh the page.");
                        window.location.reload(false);
                        return;
                    }

                    var users = data[action];
                    var value = users.Value;
                    if (!(value instanceof Array) || value.length == 0) {
                        return;
                    }
                    
                    $.each(value, function (i, user) {
                        var userHtml = '<div class="element ' + (user.ObjectType == "Teacher" ? "teacher-bg" : "student-bg") + '">' +
                                           '<div class="userimg">' +
                                               '<img src="/images/header-default.jpg" realheader="' + '/Photo/UserPhoto/' + user.O365UserId + '" />' +
                                           '</div>' +
                                           '<div class="username">' + user.DisplayName + '</div>' +
                                       '</div>';
                        $(userHtml).appendTo(content);
                    });

                    var newNextLink = users.NextLink;
                    nextLinkElement.val(newNextLink);
                    hasNextLink = typeof (newNextLink) == "string" && newNextLink.length > 0;
                    showPage(false, targetPageNum, hasNextLink, prevElement, nextElement, curPageElement, content);
                },
                complete: function (XMLHttpRequest, textStatus, errorThrown) {
                    prevNext.removeClass("disabled");
                }
            });
        }
    });

    function loadImages() {
        $("img[realheader]").each(function (i, e) {
            var $e = $(e);
            $e.attr("src", $e.attr("realheader"));
        });
    }

    function showPage(isBackward, targetPageNum, hasNextPage, prevElement, nextElement, curPageElement, content) {
        var start = (targetPageNum - 1) * 12;
        var end = targetPageNum * 12;
        var elements = content.children();
        elements.hide().slice(start, end).fadeIn("slow", function () {
            var img = $(this).find("img[realheader]");
            img.attr("src", img.attr("realheader"));
        });

        nextElement.toggleClass("current", !isBackward && end >= elements.length && !hasNextPage);
        prevElement.toggleClass("current", start === 0);
        curPageElement.val(targetPageNum);
    }

    $("#btnsearch").click(function () {
        search();
    });

    $('.txtsearch').on('keypress', function (e) {
        if (e.which === 13) {
            search();
        }
    });
    function search(isReset) {
        var queryString;
        if (isReset) {
            queryString = "";
            $(".txtsearch").val("");
        } else {
            queryString = $(".txtsearch").val();
        }
        var currentFilter = $(".filterlink-container .selected").text().trim().toLocaleLowerCase();
        var selector = "users";
        switch (currentFilter)
        {
            case "teachers":
                selector = "teachers";
                break;
            case "students":
                selector = "students";
                break;
            default:
                selector = "users";
                break;
        }
        if (queryString) {
            $("#"+selector + " .username").each(function () {
                if ($(this).text().search(new RegExp(queryString, "i")) < 0) {
                    $(this).closest(".element").hide();
                } else {
                    $(this).closest(".element").show();
                }
            });
        }
        else {
            $("#"+selector + " .element").show();
        }
    }
    var tabname = '';
    if ($(".filterlink-container a.selected").length > 0) {
        tabname = $(".filterlink-container a.selected").attr("id");
    }
    showDemoHelper(tabname);
});

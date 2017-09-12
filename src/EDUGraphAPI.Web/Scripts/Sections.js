/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
$(document).ready(function () {
    function bindShowDetail(tiles) {
        tiles.hover(function inFn(e) {
            $(this).children().last().show();
        }, function outFn(e) {
            $(this).children().last().hide();
        }).find(".detail #termdate").each(function (i, e) {
            var $e = $(e);
            var dateStr = $e.text();
            if (dateStr) {
                $e.text(moment.utc(dateStr).local().format('MMMM D YYYY'));
            }
        });
    };

    function hasSection(section, sections) {
        if (!(sections instanceof Array)) {
            return false;
        }
        var result = false;
        $.each(sections, function (i, s) {
            if (section.Email == s.Email) {
                return result = true;
            }
        });
        return result;
    }

    bindShowDetail($(".section-tiles .tile-container"));
    var tabname = '';
    if ($(".sections .filterlink-container .selected").length > 0) {
        tabname = $(".sections .filterlink-container .selected").attr("id");
    }
    showDemoHelper(tabname);

    $(".sections .filterlink-container .filterlink").click(function () {
        tabname = $(this).attr("id");
        showDemoHelper(tabname);
        search(true);
        var element = $(this);
        element.addClass("selected").siblings("a").removeClass("selected");
        var filterType = element.data("type");
        var tilesContainer = $(".sections .tiles-root-container");
        tilesContainer.removeClass(tilesContainer.attr("class").replace("tiles-root-container", "")).addClass(filterType + "-container");
    });

    $("#see-more  span").click(function () {
        search(true);
        var element = $(this);
        if (element.hasClass("disabled") || element.hasClass("nomore")) {
            return;
        }

        var schoolId = element.siblings("input#schoolid").val();
        var url = "/Schools/" + schoolId + "/Classes/Next";
        var nextLinkElement = element.siblings("input#nextlink");

        element.addClass("disabled");
        $.ajax({
            type: 'GET',
            url: url,
            dataType: 'json',
            data: { schoolId: schoolId, nextLink: nextLinkElement.val() },
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.error === "AdalException" || data.error === "Unauthorized") {
                    alert("Your current session has expired. Please click OK to refresh the page.");
                    window.location.reload(false);
                    return;
                }

                var tiles = element.parent().prev(".content");
                var newTiles = $();
                $.each(data.Sections.Value, function (i, s) {
                    var isMine = hasSection(s, data.MySections);
                    var newTile = $('<div class="tile-container"></div>');
                    var tileContainer = newTile;
                    if (isMine) {
                        tileContainer = $('<a class="mysectionlink" href="/Schools/' + schoolId + '/Classes/' + s.ObjectId + '"></a>').appendTo(newTile);
                    }
                    var tile = $('<div class="tile"><h5>' + s.DisplayName + '</h5><h2>' + s.CombinedCourseNumber + '</h2></div>');
                    tile.appendTo(tileContainer);
                    var tileDetail = $('<div class="detail" style="display: none;">' +
                                            '<h5>Course Id:</h5>' +
                                            '<h6>' + s.CourseId + '</h6>' +
                                            '<h5>Description:</h5>' +
                                            '<h6>' + s.CourseDescription + '</h6>' +
                                            '<h5>Teachers:</h5>' +
                                            ((s.Members instanceof Array) ?
                                            s.Members.reduce(function (accu, cur) {
                                                if (cur.ObjectType == 'Teacher') {
                                                    accu += '<h6>' + cur.DisplayName + '</h6>';
                                                }
                                                return accu;
                                            }, '') : '') +

                                            '<h5>Term Name:</h5>' +
                                            '<h6>' + s.TermName + '</h6>' +
                                            '<h5>Start/Finish Date:</h5>' +
                                            ((s.TermStartDate || s.TermEndDate) ?
                                            ('<h6><span id="termdate">' + s.TermStartDate + '</span>' +
                                            '<span> - </span>' +
                                            '<span id="termdate">' + s.TermEndDate + '</span>' +
                                            '</h6>') : '') +
                                            '<h5>Period:</h5>' +
                                            '<h6>' + s.Period + '</h6>' +
                                        '</div>');
                    tileDetail.appendTo(newTile);
                    newTiles = newTiles.add(newTile);
                });
                newTiles.appendTo(tiles).hide().fadeIn("slow");
                bindShowDetail(newTiles);

                var newNextLink = data.Sections.NextLink;
                nextLinkElement.val(newNextLink);
                if (typeof (newNextLink) != "string" || newNextLink.length == 0) {
                    element.addClass("nomore");
                }
                $(window).scrollTop($(document).height() - $(window).height())
            },
            complete: function () {
                element.removeClass("disabled");
            }
        });
    });

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
        if (queryString) {
            $(".tile-container h2").each(function () {
                if ($(this).text().search(new RegExp(queryString, "i")) < 0) {
                    $(this).closest(".tile-container").hide();
                } else {
                    $(this).closest(".tile-container").show();
                }
            });
        }
        else {
            $(".tile-container").show();
        }
    }
});

;
(function () {
    var _classId = $("#hidSectionid").val();
    var _isStudent = $("#hideIsStudent").val() === "True";
    var _assignment_api = {
        storedFiles: [],
        storedFilesCount:0,        
        iniAssignments: function () {
            $(".assignment-alert").hide();
            $("a.detaillink").click(function () {
                event.preventDefault();
                _assignment_api.resetAssignmentDetailForm();

                var assignmentId = $(this).data("id");
                var dueDateUTC = $(this).data("duedate");
                var assignmentDueDate = moment.utc(dueDateUTC).format("M/DD/YYYY");
                var assignmentTitle = $(this).data("title");
                var assignmentStatus = $(this).data("status");
                
                if (_isStudent) {
                    var isallowlate = $(this).data("allowlate") === "True";
                    var disableButton = false;
                    if (!isallowlate && moment.utc(dueDateUTC) < moment()) {
                        disableButton = true;
                    }
                    _assignment_api.showAssignmentDetailForStudent(assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus, disableButton, isallowlate);
                }
                else {
                    _assignment_api.showAssignmentDetailForTeacher(assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus);
                }

            });
            $("a.submissionslink").click(function () {
                event.preventDefault();
                var assignmentId = $(this).data("id");
                var assignmentDueDate = $(this).data("duedate");
                var assignmentTitle = $(this).data("title");
                var assignmentStatus = $(this).data("status");
                _assignment_api.showAssignmentSubmissions(assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus);
            });
            //assignment detail click event
            $("#assignment-detail-form .btn-save").click(function () {
                _assignment_api.updateAssignmentDetail('draft');
            });
            $("#assignment-detail-form .btn-publish").click(function () {
                _assignment_api.updateAssignmentDetail('assigned');
            });
            $("#assignment-detail-form .btn-update").click(function() {
                _assignment_api.updateAssignmentDetail('');
            });
            $("#assignment-detail-form .btn-submit").click(function () {
                _assignment_api.updateAssignmentDetail('');
            });
            
            $("#assignment-detail-form .btn-new").click(function () {
                $("#assignment-detail-form .resource-upload input:last-child").click();
            });
            $("#assignment-detail-form .btn-upload").click(function () {
                $("#assignment-detail-form .resource-upload input:last-child").click();
            });
            $("input[id^='newResourceFileCtrl']").change(function (e) {
                AssignmentPlugin.newResourceFileChange(event)
            });
            $("input[id^='newHandInResourceFileCtrl']").change(function (e) {
                AssignmentPlugin.newResourceFileChange(event)
            });

            //Add new assignment button clicked.
            $(".addassignment a").click(function () {
                $("#new-assignment").modal("show");
            });
            $("#new-assignment .btn-save").click(function () {
                _assignment_api.saveNewAssignmentSubmit('draft');
            });
            $("#new-assignment .btn-publish").click(function () {
                _assignment_api.saveNewAssignmentSubmit('assigned');
            });
            $("#new-assignment .btn-cancel").click(function () {
                _assignment_api.resetForm();
            });
            $("input[id^='fileToUpload']").change(function (e) {
                _assignment_api.doReCreateUploadControl(e);
            });
            $("#duedate").datepicker();

        },
        saveNewAssignmentSubmit: function (status) {
            var name = $("#name").val();
            var duedate = $("#duedate").val();
            if (!name || name === '') {
                var $alertContrl = $("#new-assignment .assignment-alert");
                $alertContrl.find("span").html("The Name field is required.");
                $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                    $alertContrl.slideUp(500);
                });
                return;
            }

            if (!duedate || duedate === '') {
                var $alertContrl = $("#new-assignment .assignment-alert");
                $alertContrl.find("span").html("The Due Date field is required.");
                $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                    $alertContrl.slideUp(500);
                });
                return;
            }

            $("#status").val(status);
            $("#new-assignment-form").submit();
            $("#new-assignment button").each(function () {
                $(this).attr('disabled', 'true');
            });
            $("input[name='fileUpload']").attr('disabled', 'true');
        },
        updateAssignmentDetail: function (status) {
            if (!_isStudent) {
                if (status.length > 0) {
                    $("#assignment-detail-form input[name='assignmentStatus']").val(status);
                }
                $("#assignment-detail-form-teacher").submit();
            }
            else {
                $("#assignment-detail-form-student").submit();
            }

            $("#assignment-detail-form button").each(function () {
                $(this).attr('disabled', 'true');
            });
            $("input[name='newResource']").attr('disabled', 'true');
            
        },
        //Recreate upload contorl on new assignment form.
        doReCreateUploadControl: function (e) {
            _assignment_api.storedFilesCount += 1;
            var selDiv = document.querySelector("#selectedFiles");
            if (!e.target.files) return;
            files = e.target.files;
            if (_assignment_api.storedFiles.length == 5) {
                var $alertContrl = $("#new-assignment .assignment-alert");
                $alertContrl.find("span").html("You may only attach up to 5 files.");
                $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                    $alertContrl.slideUp(500);
                });
                return;
            }
            for (var i = 0; i < files.length; i++) {
                var f = files[i];
                if (!_assignment_api.storedFiles.includes(f.name)) {
                    selDiv.innerHTML += "<div>" + f.name + "</div>";
                    _assignment_api.storedFiles.push(f.name);
                }
                else {
                    var $alertContrl = $("#new-assignment .assignment-alert");
                    $alertContrl.find("span").html("A file named " + f.name + " is already attached.");
                    $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                        $alertContrl.slideUp(500);
                    });
                }
            }
            $('#FilesToBeUploaded').val(_assignment_api.storedFiles);
            $("input[id^='fileToUpload']").hide();

            $('<input>').attr({
                type: 'file',
                multiple: 'multiple',
                id: 'fileToUpload' + _assignment_api.storedFilesCount,
                class: 'fUpload',
                name: 'fileUpload',
                style: 'float: left',
                title: '  ',
                onchange: "AssignmentPlugin.doReCreateUploadControl(event)"
            }).appendTo('#uploaders');
        },
        
        doReCreateNewResourceControl: function (e,resourceList, itemhtml, $alertContrl,  $fileContrl, fileContrlId, fileContrlName, fileContrlParent) {
            _assignment_api.storedFilesCount += 1;
            
            if(!e.target.files) return;
            files = e.target.files;

            if ($(resourceList).find(".emptyHint").length > 0) {
                $(resourceList).find(".emptyHint").remove()
            }
            if ($(resourceList).find(itemhtml).length >= 5) {
                $alertContrl.find("span").html("You may only attach up to 5 files.");
                $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                    $alertContrl.slideUp(500);
                });
            }
            else {
               for(var i = 0; i < files.length; i++) {
                    var f = files[i];
                    var bexist = false;
                    $(resourceList).find(itemhtml).each(function () {
                        if ($(this).html() === f.name) {
                            bexist = true;
                            return false;
                        }
                    })
                    if (bexist) {
                        $alertContrl.find("span").html("A file named " + f.name + " is already attached.");
                        $alertContrl.fadeTo(2000, 500).slideUp(500, function () {
                            $alertContrl.slideUp(500);
                        });
                    }
                    else {
                        $("<" + itemhtml + ">" + f.name + "</" + itemhtml + ">").appendTo(resourceList);
                        _assignment_api.storedFiles.push(f.name);
                    }
                }
            }


            //$resourceNameListInput.val(_assignment_api.storedFiles);
            $fileContrl.hide();

            $('<input>').attr({
                type: 'file',
                id: fileContrlId + _assignment_api.storedFilesCount,
                class: 'fUpload hidden',
                name: fileContrlName,
                style: 'float: left',
                title: '  ',
                onchange: "AssignmentPlugin.newResourceFileChange(event)"
            }).appendTo(fileContrlParent);
        },

        newResourceFileChange: function (e) {
            if (_isStudent) {
                AssignmentPlugin.doReCreateNewResourceControl(e,
                    "#assignment-detail-form .handin-list",
                    "li",
                    $("#assignment-detail-form .assignment-alert"),
                    $("input[id^='newResourceFileCtrl']"),
                    "newResourceFileCtrl",
                    "newResource",
                    "#assignment-detail-form .resource-upload")
            }
            else {
                AssignmentPlugin.doReCreateNewResourceControl(e,
                "#assignment-detail-form .resource-list",
                "li",
                $("#assignment-detail-form .assignment-alert"),
                $("input[id^='newResourceFileCtrl']"),
                "newResourceFileCtrl",
                "newResource",
                "#assignment-detail-form .resource-upload");
            }
        },
        showAssignmentSubmissions: function (assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus) {
            var submissionsform = $("#assignment-submissions-form");
            submissionsform.modal("show");
            submissionsform.find(".assignment-title").text(assignmentTitle);
            submissionsform.find(".due-date").text("Due Date: " + assignmentDueDate);
            $("#assignment-submissions-form tbody").html("<tr><td colspan='2'>Loading...</tr></td>");
            $.ajax({
                type: 'GET',
                url: '/Classes/' + _classId + '/Assignments/' + assignmentId + '/Submissions',
                success: function (data) {
                    var resourcesListHtml = "";
                    if (data && data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            resourcesListHtml +="<tr>"
                            resourcesListHtml += '<td>' + data[i].SubmittedBy.User.DisplayName + '</td><td>' + (data[i].SubmittedDateTime.length == 0 ? 'None' : data[i].SubmittedDateTime) + '</td>';
                            if (data[i].Resources) {
                                if (data[i].Resources.length > 0) {
                                    for (var j = 0; j < data[i].Resources.length; j++) {
                                        if (data[i].Resources[j].Resource && data[i].Resources[j].Resource.DisplayName) {
                                            resourcesListHtml += "<tr><td colspan='2' class='files'>" + data[i].Resources[j].Resource.DisplayName + "</td></tr>";
                                        }
                                    }
                                } //else {
                                   // resourcesListHtml += "<tr><td colspan='2' class='files'>There is no resources available for this user.</td></tr>";
                                //}
                            }
                            resourcesListHtml += "</tr>"
                        }
                    } //else {
                       // resourcesListHtml = "<tr><td colspan='2'>There is no data available for this page at this time.</td></tr>";
                    //}
                    $("#assignment-submissions-table tbody").html(resourcesListHtml);
                }
            });
        },
        
        showAssignmentDetailForTeacher: function (assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus) {
            var sectionId = _classId;
            var detailForm = $("#assignment-detail-form");
            detailForm.find(".assignment-title").text(assignmentTitle);
            detailForm.find(".due-date").text("Due Date: " + assignmentDueDate);
            detailForm.find(".resources-title").text("Resources for " + assignmentTitle);
            if (assignmentStatus === "draft") {
                detailForm.find(".modal-footer .btn").hide();
                detailForm.find(".modal-footer .btn:not(.btn-update)").show();
            }
            else {
                detailForm.find(".modal-footer .btn").hide();
                detailForm.find(".modal-footer .btn-update").show();
                detailForm.find(".modal-footer .btn-cancel").show();
            }
            detailForm.modal("show");
            detailForm.find(".resource-list").html("<li>Loading...</li>");
            $("input[name='assignmentId']").val(assignmentId);
            $("input[name='assignmentStatus']").val(assignmentStatus);

            $.ajax({
                type: 'GET',
                url: '/Classes/' + sectionId + '/Assignments/' + assignmentId + '/Resources',
                success: function (data) {

                    var resourcesListHtml = "";
                    if (data && data.length>0) {
                        for (var i = 0; i < data.length; i++) {
                            resourcesListHtml += '<li data-id="' + data[i].Id + '">' + data[i].Resource.DisplayName + '</li>';
                        }
                    } //else {
                        //resourcesListHtml = "<li class='emptyHint'>There is no data available for this page at this time.</li>";
                    //}
                    detailForm.find(".resource-list").html(resourcesListHtml);
                }
            });

        },
        showAssignmentDetailForStudent: function (assignmentId, assignmentTitle, assignmentDueDate, assignmentStatus, disableButton, isallowlate) {
            var sectionId = _classId;
            var detailForm = $("#assignment-detail-form");
            detailForm.find(".assignment-title").text(assignmentTitle);
            detailForm.find(".due-date").text("Due Date: " + assignmentDueDate);
            detailForm.find(".allow-late").text("Late turn-in allowed: " + (isallowlate ? "Yes": "No"));
            
            detailForm.find(".resources-title").text("Resources for " + assignmentTitle);
            detailForm.find(".handin-title").text("Hand ins for " + assignmentTitle);
            
            detailForm.find(".resource-list").html("<li>Loading...</li>");
            detailForm.find(".handin-list").html("<li>Loading...</li>");
            detailForm.modal("show");

            $("input[name='assignmentId']").val(assignmentId);
            $("input[name='assignmentStatus']").val(assignmentStatus);

            detailForm.find(".btn-submit").attr("disabled", disableButton);
            detailForm.find(".btn-upload").attr("disabled", disableButton);
            

            $.ajax({
                type: 'GET',
                url: '/Classes/' + sectionId + '/Assignments/' + assignmentId + '/ResourcesSubmission',
                success: function (data) {
                    var resourcesListHtml = "";
                    if (data.resources && data.resources.length > 0) {
                        for (var i = 0; i < data.resources.length; i++) {
                            resourcesListHtml += '<li data-id="' + data.resources[i].Id + '">' + data.resources[i].Resource.DisplayName + '</li>';
                        }
                    } //else {
                    //    resourcesListHtml = "<li>There is no data available for this page at this time.</li>";
                    //}
                    detailForm.find(".resource-list").html(resourcesListHtml);

                    var submissionsListHtml = "";
                    if (data.submission) {
                        $("input[name='submissionId']").val(data.submission.Id);
                    }
                    if (data.submission && data.submission.Resources && data.submission.Resources.length > 0) {
                        for (var i = 0; i < data.submission.Resources.length; i++) {
                            submissionsListHtml += '<li data-id="' + data.submission.Resources[i].Id + '">' + data.submission.Resources[i].Resource.DisplayName + '</li>';
                        }
                    } //else {
                       // submissionsListHtml = "<li class='emptyHint'>There is no data available for this page at this time.</li>";
                    //}
                    detailForm.find(".handin-list").html(submissionsListHtml);
                }
            });

        },
        resetForm: function () {
            form = $("#new-assignment").find("form");
            form[0].reset();
            $("#selectedFiles").html('');
            $("#errorMessages").html('');
            $("#status").val('');
            $("#new-assignment").modal("hide");
            _assignment_api.storedFilesCount = 0;
            _assignment_api.storedFiles = [];
        },
        resetAssignmentDetailForm: function () {
            $("input[name='resourceNameListInput']").html('');
            //$("#errorMessages").html('');
            _assignment_api.storedFilesCount = 0;
            _assignment_api.storedFiles = [];
        }
    }
    this.AssignmentPlugin = _assignment_api;
})();
$(function () {
    //$('.nav-tabs a[href="#assignments"]').tab('show');
    AssignmentPlugin.iniAssignments();
    
});
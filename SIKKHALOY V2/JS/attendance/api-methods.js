const attendance = (function() {
    const baseUrl = "http://localhost:19362/api";

    //create new user
    const registerNewDeviceUser = function () {
        var isSend = false;
        const index = $("[id*=School_DropDownList]").get(0).selectedIndex;
        const password = $('[id*=Password_TextBox]').val();

        if (index > 0 && password !== '') {
            const users = {
                username: $('[id*=School_DropDownList]').val(),
                email: $('[id*=School_DropDownList]').val() + "@gmail.com",
                password: password,
                confirmPassword: password
            };

            $.ajax({
                url: `${baseUrl}/account/register`,
                method: 'POST',
                async: false,
                contentType: 'application/json',
                data: JSON.stringify(users),
                success: function () {
                    isSend = true;
                },
                error: function (err) {
                    var response = null;
                    const errors = [];
                    var errorsString = "";

                    if (err.status === 400) {
                        try {
                            response = JSON.parse(err.responseText);
                        } catch (e) { }
                    }
                    if (response != null) {
                        const modelState = response.ModelState;

                        for (let key in modelState) {
                            if (modelState.hasOwnProperty(key)) {
                                errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                                errors.push(modelState[key]); //list of error messages in an array
                            }
                        }
                    }

                    //DISPLAY THE LIST OF ERROR MESSAGES 
                    if (errorsString !== "") {
                        $("#divErrorText").html(errorsString);
                        $('#divError').show('fade');
                    }
                }
            });
        }
        return isSend;
    }

    //change device password
    const changeDevicePassword = function (model) {
        $.ajax({
            url: `${baseUrl}/account/ChangePassword`,
            method: 'POST',
            async: false,
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function () {
                $("#error-response").html("Password Changed Success!")
            },
            error: function (err) {
                var response = null;
                const errors = [];
                var errorsString = "";

                if (err.status === 400) {
                    try {
                        response = JSON.parse(err.responseText);
                    } catch (e) { }
                }
                if (response != null) {
                    const modelState = response.ModelState;

                    for (let key in modelState) {
                        if (modelState.hasOwnProperty(key)) {
                            errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                            errors.push(modelState[key]); //list of error messages in an array
                        }
                    }
                }

                //DISPLAY THE LIST OF ERROR MESSAGES 
                if (errorsString !== "") {
                    $("#error-response").html(errorsString);
                }
            }
        });
    }

    const obj = {};
    obj.registerNewDeviceUser = registerNewDeviceUser;
    obj.changeDevicePassword = changeDevicePassword;


    return obj;
})();
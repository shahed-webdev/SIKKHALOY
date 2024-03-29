// Input 0
(function (f) {
    "object" === typeof exports && exports && "object" === typeof module && module && module.exports === exports ? f(require("jquery")) : "function" === typeof define && define.amd ? define(["jquery"], f) : f(jQuery);
})(function (f) {
    function n(a) {
        a = a[0];
        return 0 < a.offsetWidth && 0 < a.offsetHeight;
    }
    function D(a) {
        a.minTime && (a.minTime = h(a.minTime));
        a.maxTime && (a.maxTime = h(a.maxTime));
        a.durationTime && "function" !== typeof a.durationTime && (a.durationTime = h(a.durationTime));
        "now" == a.scrollDefault ? a.scrollDefault = h(new Date) : a.scrollDefault ? a.scrollDefault = h(a.scrollDefault) : a.minTime && (a.scrollDefault = a.minTime);
        a.scrollDefault && (a.scrollDefault = E(a.scrollDefault, a));
        "string" === f.type(a.timeFormat) && a.timeFormat.match(/[gh]/) && (a._twelveHourTime = !0);
        if (0 < a.disableTimeRanges.length) {
            for (var b in a.disableTimeRanges) {
                a.disableTimeRanges[b] = [h(a.disableTimeRanges[b][0]), h(a.disableTimeRanges[b][1])];
            }
            a.disableTimeRanges = a.disableTimeRanges.sort(function (a, b) {
                return a[0] - b[0];
            });
            for (b = a.disableTimeRanges.length - 1; 0 < b; b--) {
                a.disableTimeRanges[b][0] <= a.disableTimeRanges[b - 1][1] && (a.disableTimeRanges[b - 1] = [Math.min(a.disableTimeRanges[b][0], a.disableTimeRanges[b - 1][0]), Math.max(a.disableTimeRanges[b][1], a.disableTimeRanges[b - 1][1])], a.disableTimeRanges.splice(b, 1));
            }
        }
        return a;
    }
    function x(a) {
        var b = a.data("timepicker-settings"), c = a.data("timepicker-list");
        c && c.length && (c.remove(), a.data("timepicker-list", !1));
        if (b.useSelect) {
            var d = c = f("<select />", { "class": "ui-timepicker-select" })
        } else {
            c = f("<ul />", { "class": "ui-timepicker-list" }), d = f("<div />", { "class": "ui-timepicker-wrapper", tabindex: -1 }), d.css({ display: "none", position: "absolute" }).append(c);
        }
        if (b.noneOption) {
            if (!0 === b.noneOption && (b.noneOption = b.useSelect ? "Time..." : "None"), f.isArray(b.noneOption)) {
                for (var e in b.noneOption) {
                    if (parseInt(e, 10) == e) {
                        var k = F(b.noneOption[e], b.useSelect);
                        c.append(k);
                    }
                }
            } else {
                k = F(b.noneOption, b.useSelect), c.append(k);
            }
        }
        b.className && d.addClass(b.className);
        null === b.minTime && null === b.durationTime || !b.showDuration || (d.addClass("ui-timepicker-with-duration"), d.addClass("ui-timepicker-step-" + b.step));
        k = b.minTime;
        "function" === typeof b.durationTime ? k = h(b.durationTime()) : null !== b.durationTime && (k = b.durationTime);
        e = null !== b.minTime ? b.minTime : 0;
        var g = null !== b.maxTime ? b.maxTime : e + 86400 - 1;
        g <= e && (g += 86400);
        86399 === g && "string" === f.type(b.timeFormat) && -1 !== b.timeFormat.indexOf("H") && (g = 86400);
        for (var u = b.disableTimeRanges, p = 0, r = u.length; e <= g; e += 60 * b.step) {
            var q = e, w = v(q, b.timeFormat);
            if (b.useSelect) {
                var m = f("<option />", { value: w })
            } else {
                m = f("<li />"), m.data("time", 86400 >= q ? q : q % 86400);
            }
            m.text(w);
            if ((null !== b.minTime || null !== b.durationTime) && b.showDuration) {
                if (w = I(e - k, b.step), b.useSelect) {
                    m.text(m.text() + " (" + w + ")");
                } else {
                    var n = f("<span />", { "class": "ui-timepicker-duration" });
                    n.text(" (" + w + ")");
                    m.append(n);
                }
            }
            p < r && (q >= u[p][1] && (p += 1), u[p] && q >= u[p][0] && q < u[p][1] && (b.useSelect ? m.prop("disabled", !0) : m.addClass("ui-timepicker-disabled")));
            c.append(m);
        }
        d.data("timepicker-input", a);
        a.data("timepicker-list", d);
        b.useSelect ? (a.val() && c.val(y(a.val(), b)), c.on("focus", function () {
            f(this).data("timepicker-input").trigger("showTimepicker");
        }), c.on("blur", function () {
            f(this).data("timepicker-input").trigger("hideTimepicker");
        }), c.on("change", function () {
            t(a, f(this).val(), "select");
        }), t(a, c.val()), a.hide().after(c)) : (b = b.appendTo, "string" === typeof b ? b = f(b) : "function" === typeof b && (b = b(a)), b.append(d), z(a, c), c.on("mousedown", "li", function (b) {
            a.off("focus.timepicker");
            a.on("focus.timepicker-ie-hack", function () {
                a.off("focus.timepicker-ie-hack");
                a.on("focus.timepicker", l.show);
            });
            A(a) || a[0].focus();
            c.find("li").removeClass("ui-timepicker-selected");
            f(this).addClass("ui-timepicker-selected");
            B(a) && (a.trigger("hideTimepicker"), d.hide());
        }));
    }
    function F(a, b) {
        var c, d, e;
        "object" == typeof a ? (c = a.label, d = a.className, e = a.value) : "string" == typeof a ? c = a : f.error("Invalid noneOption value");
        return b ? f("<option />", { value: e, "class": d, text: c }) : f("<li />", { "class": d, text: c }).data("time", e);
    }
    function E(a, b) {
        f.isNumeric(a) || (a = h(a));
        if (null === a) {
            return null;
        }
        var c = a % (60 * b.step);
        return c >= 30 * b.step ? a + (60 * b.step - c) : a - c;
    }
    function y(a, b) {
        a = E(a, b);
        if (null !== a) {
            return v(a, b.timeFormat);
        }
    }
    function G(a) {
        a = f(a.target);
        0 === a.closest(".ui-timepicker-input").length && 0 === a.closest(".ui-timepicker-wrapper").length && (l.hide(), f(document).unbind(".ui-timepicker"));
    }
    function A(a) {
        a = a.data("timepicker-settings");
        return (window.navigator.msMaxTouchPoints || "ontouchstart" in document) && a.disableTouchKeyboard;
    }
    function C(a, b, c) {
        if (!c && 0 !== c) {
            return !1;
        }
        var d = !1, e = 30 * a.data("timepicker-settings").step;
        b.find("li").each(function (a, b) {
            var g = f(b);
            if ("number" == typeof g.data("time")) {
                var h = g.data("time") - c;
                if (Math.abs(h) < e || h == e) {
                    return d = g, !1;
                }
            }
        });
        return d;
    }
    function z(a, b) {
        b.find("li").removeClass("ui-timepicker-selected");
        var c = h(r(a), a.data("timepicker-settings"));
        if (null !== c && (c = C(a, b, c))) {
            var d = c.offset().top - b.offset().top;
            (d + c.outerHeight() > b.outerHeight() || 0 > d) && b.scrollTop(b.scrollTop() + c.position().top - c.outerHeight());
            c.addClass("ui-timepicker-selected");
        }
    }
    function H(a, b) {
        if ("" !== this.value && "timepicker" != b) {
            var c = f(this);
            c.data("timepicker-list");
            if (!c.is(":focus") || a && "change" == a.type) {
                var d = h(this.value);
                if (null === d) {
                    c.trigger("timeFormatError");
                } else {
                    var e = c.data("timepicker-settings"), k = !1;
                    null !== e.minTime && d < e.minTime ? k = !0 : null !== e.maxTime && d > e.maxTime && (k = !0);
                    f.each(e.disableTimeRanges, function () {
                        if (d >= this[0] && d < this[1]) {
                            return k = !0, !1;
                        }
                    });
                    if (e.forceRoundTime) {
                        var g = d % (60 * e.step), d = g >= 30 * e.step ? d + (60 * e.step - g) : d - g
                    }
                    e = v(d, e.timeFormat);
                    k ? t(c, e, "error") && c.trigger("timeRangeError") : t(c, e);
                }
            }
        }
    }
    function r(a) {
        return a.is("input") ? a.val() : a.data("ui-timepicker-value");
    }
    function t(a, b, c) {
        if (a.is("input")) {
            a.val(b);
            var d = a.data("timepicker-settings");
            d.useSelect && "select" != c && a.data("timepicker-list").val(y(b, d));
        }
        if (a.data("ui-timepicker-value") != b) {
            return a.data("ui-timepicker-value", b), "select" == c ? a.trigger("selectTime").trigger("changeTime").trigger("change", "timepicker") : "error" != c && a.trigger("changeTime"), !0;
        }
        a.trigger("selectTime");
        return !1;
    }
    function J(a) {
        var b = f(this), c = b.data("timepicker-list");
        if (!c || !n(c)) {
            if (40 == a.keyCode) {
                l.show.call(b.get(0)), c = b.data("timepicker-list"), A(b) || b.focus();
            } else {
                return !0;
            }
        }
        switch (a.keyCode) {
            case 13:
                return B(b) && l.hide.apply(this), a.preventDefault(), !1;
            case 38:
                var d = c.find(".ui-timepicker-selected");
                d.length ? d.is(":first-child") || (d.removeClass("ui-timepicker-selected"), d.prev().addClass("ui-timepicker-selected"), d.prev().position().top < d.outerHeight() && c.scrollTop(c.scrollTop() - d.outerHeight())) : (c.find("li").each(function (a, b) {
                    if (0 < f(b).position().top) {
                        return d = f(b), !1;
                    }
                }), d.addClass("ui-timepicker-selected"));
                return !1;
            case 40:
                return d = c.find(".ui-timepicker-selected"), 0 === d.length ? (c.find("li").each(function (a, b) {
                    if (0 < f(b).position().top) {
                        return d = f(b), !1;
                    }
                }), d.addClass("ui-timepicker-selected")) : d.is(":last-child") || (d.removeClass("ui-timepicker-selected"), d.next().addClass("ui-timepicker-selected"), d.next().position().top + 2 * d.outerHeight() > c.outerHeight() && c.scrollTop(c.scrollTop() + d.outerHeight())), !1;
            case 27:
                c.find("li").removeClass("ui-timepicker-selected");
                l.hide();
                break;
            case 9:
                l.hide();
                break;
            default:
                return !0;
        }
    }
    function K(a) {
        var b = f(this), c = b.data("timepicker-list");
        if (!c || !n(c)) {
            return !0;
        }
        if (!b.data("timepicker-settings").typeaheadHighlight) {
            return c.find("li").removeClass("ui-timepicker-selected"), !0;
        }
        switch (a.keyCode) {
            case 96:
                ;
            case 97:
                ;
            case 98:
                ;
            case 99:
                ;
            case 100:
                ;
            case 101:
                ;
            case 102:
                ;
            case 103:
                ;
            case 104:
                ;
            case 105:
                ;
            case 48:
                ;
            case 49:
                ;
            case 50:
                ;
            case 51:
                ;
            case 52:
                ;
            case 53:
                ;
            case 54:
                ;
            case 55:
                ;
            case 56:
                ;
            case 57:
                ;
            case 65:
                ;
            case 77:
                ;
            case 80:
                ;
            case 186:
                ;
            case 8:
                ;
            case 46:
                z(b, c);
        }
    }
    function B(a) {
        var b = a.data("timepicker-settings"), c = null, d = a.data("timepicker-list").find(".ui-timepicker-selected");
        if (d.hasClass("ui-timepicker-disabled")) {
            return !1;
        }
        d.length && (c = d.data("time"));
        null !== c && ("string" == typeof c ? (a.val(c), a.trigger("selectTime").trigger("changeTime").trigger("change", "timepicker")) : (b = v(c, b.timeFormat), t(a, b, "select")));
        return !0;
    }
    function I(a, b) {
        a = Math.abs(a);
        var c = Math.round(a / 60), d = [], e;
        60 > c ? d = [c, g.mins] : (e = Math.floor(c / 60), c %= 60, 30 == b && 30 == c && (e += g.decimal + 5), d.push(e), d.push(1 == e ? g.hr : g.hrs), 30 != b && c && (d.push(c), d.push(g.mins)));
        return d.join(" ");
    }
    function v(a, b) {
        if (null !== a) {
            var c = new Date(L.valueOf() + 1E3 * a);
            if (!isNaN(c.getTime())) {
                if ("function" === f.type(b)) {
                    return b(c);
                }
                for (var d = "", e, k = 0; k < b.length; k++) {
                    switch (e = b.charAt(k), e) {
                        case "a":
                            d += 11 < c.getHours() ? g.pm : g.am;
                            break;
                        case "A":
                            d += 11 < c.getHours() ? g.pm.toUpperCase() : g.am.toUpperCase();
                            break;
                        case "g":
                            e = c.getHours() % 12;
                            d += 0 === e ? "12" : e;
                            break;
                        case "G":
                            d += c.getHours();
                            break;
                        case "h":
                            e = c.getHours() % 12;
                            0 !== e && 10 > e && (e = "0" + e);
                            d += 0 === e ? "12" : e;
                            break;
                        case "H":
                            e = c.getHours();
                            86400 === a && (e = 24);
                            d += 9 < e ? e : "0" + e;
                            break;
                        case "i":
                            e = c.getMinutes();
                            d += 9 < e ? e : "0" + e;
                            break;
                        case "s":
                            a = c.getSeconds();
                            d += 9 < a ? a : "0" + a;
                            break;
                        case "\\":
                            k++;
                            d += b.charAt(k);
                            break;
                        default:
                            d += e;
                    }
                }
                return d;
            }
        }
    }
    function h(a, b) {
        if ("" === a) {
            return null;
        }
        if (!a || a + 0 == a) {
            return a;
        }
        if ("object" == typeof a) {
            return 3600 * a.getHours() + 60 * a.getMinutes() + a.getSeconds();
        }
        a = a.toLowerCase();
        if ("a" == a.slice(-1) || "p" == a.slice(-1)) {
            a += "m";
        }
        var c = a.match(new RegExp("^([0-2]?[0-9])\\W?([0-5][0-9])?\\W?([0-5][0-9])?\\s*(" + g.am + "|" + g.pm + ")?$"));
        if (!c) {
            return null;
        }
        var d = parseInt(1 * c[1], 10), e = c[4], f = d;
        12 >= d && e && (f = 12 == d ? c[4] == g.pm ? 12 : 0 : d + (c[4] == g.pm ? 12 : 0));
        c = 3600 * f + 60 * (1 * c[2] || 0) + (1 * c[3] || 0);
        !e && b && b._twelveHourTime && b.scrollDefault && (e = c - b.scrollDefault, 0 > e && -43200 <= e && (c = (c + 43200) % 86400));
        return c;
    }
    var L = new Date(1970, 1, 1, 0, 0, 0), g = { am: "am", pm: "pm", AM: "AM", PM: "PM", decimal: ".", mins: "mins", hr: "hr", hrs: "hrs" }, l = {
        init: function (a) {
            return this.each(function () {
                var b = f(this), c = [], d;
                for (d in f.fn.timepicker.defaults) {
                    b.data(d) && (c[d] = b.data(d));
                }
                c = f.extend({}, f.fn.timepicker.defaults, c, a);
                c.lang && (g = f.extend(g, c.lang));
                c = D(c);
                b.data("timepicker-settings", c);
                b.addClass("ui-timepicker-input");
                c.useSelect ? x(b) : (b.prop("autocomplete", "off"), b.on("click.timepicker focus.timepicker", l.show), b.on("change.timepicker", H), b.on("keydown.timepicker", J), b.on("keyup.timepicker", K), H.call(b.get(0)));
            });
        }, show: function (a) {
            var b = f(this), c = b.data("timepicker-settings");
            if (a) {
                if (!c.showOnFocus) {
                    return !0;
                }
                a.preventDefault();
            }
            if (c.useSelect) {
                b.data("timepicker-list").focus();
            } else {
                if (A(b) && b.blur(), a = b.data("timepicker-list"), !b.prop("readonly") && (a && 0 !== a.length && "function" !== typeof c.durationTime || (x(b), a = b.data("timepicker-list")), !n(a))) {
                    l.hide();
                    a.show();
                    var d = {};
                    d.left = "rtl" == c.orientation ? b.offset().left + b.outerWidth() - a.outerWidth() + parseInt(a.css("marginLeft").replace("px", ""), 10) : b.offset().left + parseInt(a.css("marginLeft").replace("px", ""), 10);
                    b.offset().top + b.outerHeight(!0) + a.outerHeight() > f(window).height() + f(window).scrollTop() ? (a.addClass("ui-timepicker-positioned-top"), d.top = b.offset().top - a.outerHeight() + parseInt(a.css("marginTop").replace("px", ""), 10)) : (a.removeClass("ui-timepicker-positioned-top"), d.top = b.offset().top + b.outerHeight() + parseInt(a.css("marginTop").replace("px", ""), 10));
                    a.offset(d);
                    d = a.find(".ui-timepicker-selected");
                    d.length || (r(b) ? d = C(b, a, h(r(b))) : c.scrollDefault && (d = C(b, a, c.scrollDefault)));
                    d && d.length ? (d = a.scrollTop() + d.position().top - d.outerHeight(), a.scrollTop(d)) : a.scrollTop(0);
                    f(document).on("touchstart.ui-timepicker mousedown.ui-timepicker", G);
                    if (c.closeOnWindowScroll) {
                        f(document).on("scroll.ui-timepicker", G);
                    }
                    b.trigger("showTimepicker");
                    return this;
                }
            }
        }, hide: function (a) {
            a = f(this);
            var b = a.data("timepicker-settings");
            b && b.useSelect && a.blur();
            f(".ui-timepicker-wrapper").each(function () {
                var a = f(this);
                if (n(a)) {
                    var b = a.data("timepicker-input"), e = b.data("timepicker-settings");
                    e && e.selectOnBlur && B(b);
                    a.hide();
                    b.trigger("hideTimepicker");
                }
            });
            return this;
        }, option: function (a, b) {
            return this.each(function () {
                var c = f(this), d = c.data("timepicker-settings"), e = c.data("timepicker-list");
                if ("object" == typeof a) {
                    d = f.extend(d, a);
                } else {
                    if ("string" == typeof a && "undefined" != typeof b) {
                        d[a] = b;
                    } else {
                        if ("string" == typeof a) {
                            return d[a];
                        }
                    }
                }
                d = D(d);
                c.data("timepicker-settings", d);
                e && (e.remove(), c.data("timepicker-list", !1));
                d.useSelect && x(c);
            });
        }, getSecondsFromMidnight: function () {
            return h(r(this));
        }, getTime: function (a) {
            var b = r(this);
            if (!b) {
                return null;
            }
            a || (a = new Date);
            b = h(b);
            a = new Date(a);
            a.setHours(b / 3600);
            a.setMinutes(b % 3600 / 60);
            a.setSeconds(b % 60);
            a.setMilliseconds(0);
            return a;
        }, setTime: function (a) {
            var b = this.data("timepicker-settings");
            a = b.forceRoundTime ? y(a, b) : v(h(a), b.timeFormat);
            t(this, a);
            this.data("timepicker-list") && z(this, this.data("timepicker-list"));
            return this;
        }, remove: function () {
            if (this.hasClass("ui-timepicker-input")) {
                var a = this.data("timepicker-settings");
                this.removeAttr("autocomplete", "off");
                this.removeClass("ui-timepicker-input");
                this.removeData("timepicker-settings");
                this.off(".timepicker");
                this.data("timepicker-list") && this.data("timepicker-list").remove();
                a.useSelect && this.show();
                this.removeData("timepicker-list");
                return this;
            }
        }
    };
    f.fn.timepicker = function (a) {
        if (!this.length) {
            return this;
        }
        if (l[a]) {
            return this.hasClass("ui-timepicker-input") ? l[a].apply(this, Array.prototype.slice.call(arguments, 1)) : this;
        }
        if ("object" !== typeof a && a) {
            f.error("Method " + a + " does not exist on jQuery.timepicker");
        } else {
            return l.init.apply(this, arguments);
        }
    };
    f.fn.timepicker.defaults = { className: null, minTime: null, maxTime: null, durationTime: null, step: 30, showDuration: !1, showOnFocus: !0, timeFormat: "g:ia", scrollDefault: null, selectOnBlur: !1, disableTouchKeyboard: !1, forceRoundTime: !1, appendTo: "body", orientation: "ltr", disableTimeRanges: [], closeOnWindowScroll: !1, typeaheadHighlight: !0, noneOption: !1 };
});

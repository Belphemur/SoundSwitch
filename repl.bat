@if (@X)==(@Y) @end /* Harmless hybrid line that begins a JScript comment

::************ Documentation ***********
::REPL.BAT version 6.2
:::
:::REPL  Search  Replace  [Options  [SourceVar]]
:::REPL  /?[REGEX|REPLACE]
:::REPL  /V
:::
:::  Performs a global regular expression search and replace operation on
:::  each line of input from stdin and prints the result to stdout.
:::
:::  Each parameter may be optionally enclosed by double quotes. The double
:::  quotes are not considered part of the argument. The quotes are required
:::  if the parameter contains a batch token delimiter like space, tab, comma,
:::  semicolon. The quotes should also be used if the argument contains a
:::  batch special character like &, |, etc. so that the special character
:::  does not need to be escaped with ^.
:::
:::  If called with a single argument of /?, then prints help documentation
:::  to stdout. If a single argument of /?REGEX, then opens up Microsoft's
:::  JScript regular expression documentation within your browser. If a single
:::  argument of /?REPLACE, then opens up Microsoft's JScript REPLACE
:::  documentation within your browser.
:::
:::  If called with a single argument of /V, case insensitive, then prints
:::  the version of REPL.BAT.
:::
:::  Search  - By default, this is a case sensitive JScript (ECMA) regular
:::            expression expressed as a string.
:::
:::            JScript regex syntax documentation is available at
:::            http://msdn.microsoft.com/en-us/library/ae5bf541(v=vs.80).aspx
:::
:::  Replace - By default, this is the string to be used as a replacement for
:::            each found search expression. Full support is provided for
:::            substituion patterns available to the JScript replace method.
:::
:::            For example, $& represents the portion of the source that matched
:::            the entire search pattern, $1 represents the first captured
:::            submatch, $2 the second captured submatch, etc. A $ literal
:::            can be escaped as $$.
:::
:::            An empty replacement string must be represented as "".
:::
:::            Replace substitution pattern syntax is fully documented at
:::            http://msdn.microsoft.com/en-US/library/efy6s3e6(v=vs.80).aspx
:::
:::  Options - An optional string of characters used to alter the behavior
:::            of REPL. The option characters are case insensitive, and may
:::            appear in any order.
:::
:::            A - Only print altered lines. Unaltered lines are discarded.
:::                If the S options is present, then prints the result only if
:::                there was a change anywhere in the string. The A option is
:::                incompatible with the M option unless the S option is present.
:::
:::            B - The Search must match the beginning of a line.
:::                Mostly used with literal searches.
:::
:::            E - The Search must match the end of a line.
:::                Mostly used with literal searches.
:::
:::            I - Makes the search case-insensitive.
:::
:::            J - The Replace argument represents a JScript expression.
:::                The expression may access an array like arguments object
:::                named $. However, $ is not a true array object.
:::
:::                The $.length property contains the total number of arguments
:::                available. The $.length value is equal to n+3, where n is the
:::                number of capturing left parentheses within the Search string.
:::
:::                $[0] is the substring that matched the Search,
:::                $[1] through $[n] are the captured submatch strings,
:::                $[n+1] is the offset where the match occurred, and
:::                $[n+2] is the original source string.
:::
:::                Arguments $[0] through $[10] may be abbreviated as
:::                $1 through $10. Argument $[11] and above must use the square
:::                bracket notation.
:::
:::            L - The Search is treated as a string literal instead of a
:::                regular expression. Also, all $ found in the Replace string
:::                are treated as $ literals.
:::
:::            M - Multi-line mode. The entire contents of stdin is read and
:::                processed in one pass instead of line by line, thus enabling
:::                search for \n. This also enables preservation of the original
:::                line terminators. If the M option is not present, then every
:::                printed line is terminated with carriage return and line feed.
:::                The M option is incompatible with the A option unless the S
:::                option is also present.
:::
:::                Note: If working with binary data containing NULL bytes,
:::                      then the M option must be used.
:::
:::            S - The source is read from an environment variable instead of
:::                from stdin. The name of the source environment variable is
:::                specified in the next argument after the option string. Without
:::                the M option, ^ anchors the beginning of the string, and $ the
:::                end of the string. With the M option, ^ anchors the beginning
:::                of a line, and $ the end of a line.
:::
:::            V - Search and Replace represent the name of environment
:::                variables that contain the respective values. An undefined
:::                variable is treated as an empty string.
:::
:::            X - Enables extended substitution pattern syntax with support
:::                for the following escape sequences within the Replace string:
:::
:::                \\     -  Backslash
:::                \b     -  Backspace
:::                \f     -  Formfeed
:::                \n     -  Newline
:::                \q     -  Quote
:::                \r     -  Carriage Return
:::                \t     -  Horizontal Tab
:::                \v     -  Vertical Tab
:::                \xnn   -  Extended ASCII byte code expressed as 2 hex digits
:::                \unnnn -  Unicode character expressed as 4 hex digits
:::
:::                Also enables the \q escape sequence for the Search string.
:::                The other escape sequences are already standard for a regular
:::                expression Search string.
:::
:::                Also modifies the behavior of \xnn in the Search string to work
:::                properly with extended ASCII byte codes.
:::
:::                Extended escape sequences are supported even when the L option
:::                is used. Both Search and Replace support all of the extended
:::                escape sequences if both the X and L opions are combined.
:::
:::  Return Codes:  0 = At least one change was made
:::                     or the /? or /V option was used
:::
:::                 1 = No change was made
:::
:::                 2 = Invalid call syntax or incompatible options
:::
:::                 3 = JScript runtime error, typically due to invalid regex
:::
::: REPL.BAT was written by Dave Benham, with assistance from DosTips user Aacini
::: to get \xnn to work properly with extended ASCII byte codes. Also assistance
::: from DosTips user penpen diagnosing issues reading NULL bytes, along with a
::: workaround. REPL.BAT was originally posted at:
::: http://www.dostips.com/forum/viewtopic.php?f=3&t=3855
:::

::************ Batch portion ***********
@echo off
if .%2 equ . (
  if "%~1" equ "/?" (
    <"%~f0" cscript //E:JScript //nologo "%~f0" "^:::" "" a
    exit /b 0
  ) else if /i "%~1" equ "/?regex" (
    explorer "http://msdn.microsoft.com/en-us/library/ae5bf541(v=vs.80).aspx"
    exit /b 0
  ) else if /i "%~1" equ "/?replace" (
    explorer "http://msdn.microsoft.com/en-US/library/efy6s3e6(v=vs.80).aspx"
    exit /b 0
  ) else if /i "%~1" equ "/V" (
    <"%~f0" cscript //E:JScript //nologo "%~f0" "^::(REPL\.BAT version)" "$1" a
    exit /b 0
  ) else (
    call :err "Insufficient arguments"
    exit /b 2
  )
)
echo(%~3|findstr /i "[^SMILEBVXAJ]" >nul && (
  call :err "Invalid option(s)"
  exit /b 2
)
echo(%~3|findstr /i "M"|findstr /i "A"|findstr /vi "S" >nul && (
  call :err "Incompatible options"
  exit /b 2
)
cscript //E:JScript //nologo "%~f0" %*
exit /b %errorlevel%

:err
>&2 echo ERROR: %~1. Use REPL /? to get help.
exit /b

************* JScript portion **********/
var rtn=1;
try {
  var env=WScript.CreateObject("WScript.Shell").Environment("Process");
  var args=WScript.Arguments;
  var search=args.Item(0);
  var replace=args.Item(1);
  var options="g";
  if (args.length>2) options+=args.Item(2).toLowerCase();
  var multi=(options.indexOf("m")>=0);
  var alterations=(options.indexOf("a")>=0);
  if (alterations) options=options.replace(/a/g,"");
  var srcVar=(options.indexOf("s")>=0);
  if (srcVar) options=options.replace(/s/g,"");
  var jexpr=(options.indexOf("j")>=0);
  if (jexpr) options=options.replace(/j/g,"");
  if (options.indexOf("v")>=0) {
    options=options.replace(/v/g,"");
    search=env(search);
    replace=env(replace);
  }
  if (options.indexOf("x")>=0) {
    options=options.replace(/x/g,"");
    if (!jexpr) {
      replace=replace.replace(/\\\\/g,"\\B");
      replace=replace.replace(/\\q/g,"\"");
      replace=replace.replace(/\\x80/g,"\\u20AC");
      replace=replace.replace(/\\x82/g,"\\u201A");
      replace=replace.replace(/\\x83/g,"\\u0192");
      replace=replace.replace(/\\x84/g,"\\u201E");
      replace=replace.replace(/\\x85/g,"\\u2026");
      replace=replace.replace(/\\x86/g,"\\u2020");
      replace=replace.replace(/\\x87/g,"\\u2021");
      replace=replace.replace(/\\x88/g,"\\u02C6");
      replace=replace.replace(/\\x89/g,"\\u2030");
      replace=replace.replace(/\\x8[aA]/g,"\\u0160");
      replace=replace.replace(/\\x8[bB]/g,"\\u2039");
      replace=replace.replace(/\\x8[cC]/g,"\\u0152");
      replace=replace.replace(/\\x8[eE]/g,"\\u017D");
      replace=replace.replace(/\\x91/g,"\\u2018");
      replace=replace.replace(/\\x92/g,"\\u2019");
      replace=replace.replace(/\\x93/g,"\\u201C");
      replace=replace.replace(/\\x94/g,"\\u201D");
      replace=replace.replace(/\\x95/g,"\\u2022");
      replace=replace.replace(/\\x96/g,"\\u2013");
      replace=replace.replace(/\\x97/g,"\\u2014");
      replace=replace.replace(/\\x98/g,"\\u02DC");
      replace=replace.replace(/\\x99/g,"\\u2122");
      replace=replace.replace(/\\x9[aA]/g,"\\u0161");
      replace=replace.replace(/\\x9[bB]/g,"\\u203A");
      replace=replace.replace(/\\x9[cC]/g,"\\u0153");
      replace=replace.replace(/\\x9[dD]/g,"\\u009D");
      replace=replace.replace(/\\x9[eE]/g,"\\u017E");
      replace=replace.replace(/\\x9[fF]/g,"\\u0178");
      replace=replace.replace(/\\b/g,"\b");
      replace=replace.replace(/\\f/g,"\f");
      replace=replace.replace(/\\n/g,"\n");
      replace=replace.replace(/\\r/g,"\r");
      replace=replace.replace(/\\t/g,"\t");
      replace=replace.replace(/\\v/g,"\v");
      replace=replace.replace(/\\x[0-9a-fA-F]{2}|\\u[0-9a-fA-F]{4}/g,
        function($0,$1,$2){
          return String.fromCharCode(parseInt("0x"+$0.substring(2)));
        }
      );
      replace=replace.replace(/\\B/g,"\\");
    }
    search=search.replace(/\\\\/g,"\\B");
    search=search.replace(/\\q/g,"\"");
    search=search.replace(/\\x80/g,"\\u20AC");
    search=search.replace(/\\x82/g,"\\u201A");
    search=search.replace(/\\x83/g,"\\u0192");
    search=search.replace(/\\x84/g,"\\u201E");
    search=search.replace(/\\x85/g,"\\u2026");
    search=search.replace(/\\x86/g,"\\u2020");
    search=search.replace(/\\x87/g,"\\u2021");
    search=search.replace(/\\x88/g,"\\u02C6");
    search=search.replace(/\\x89/g,"\\u2030");
    search=search.replace(/\\x8[aA]/g,"\\u0160");
    search=search.replace(/\\x8[bB]/g,"\\u2039");
    search=search.replace(/\\x8[cC]/g,"\\u0152");
    search=search.replace(/\\x8[eE]/g,"\\u017D");
    search=search.replace(/\\x91/g,"\\u2018");
    search=search.replace(/\\x92/g,"\\u2019");
    search=search.replace(/\\x93/g,"\\u201C");
    search=search.replace(/\\x94/g,"\\u201D");
    search=search.replace(/\\x95/g,"\\u2022");
    search=search.replace(/\\x96/g,"\\u2013");
    search=search.replace(/\\x97/g,"\\u2014");
    search=search.replace(/\\x98/g,"\\u02DC");
    search=search.replace(/\\x99/g,"\\u2122");
    search=search.replace(/\\x9[aA]/g,"\\u0161");
    search=search.replace(/\\x9[bB]/g,"\\u203A");
    search=search.replace(/\\x9[cC]/g,"\\u0153");
    search=search.replace(/\\x9[dD]/g,"\\u009D");
    search=search.replace(/\\x9[eE]/g,"\\u017E");
    search=search.replace(/\\x9[fF]/g,"\\u0178");
    if (options.indexOf("l")>=0) {
      search=search.replace(/\\b/g,"\b");
      search=search.replace(/\\f/g,"\f");
      search=search.replace(/\\n/g,"\n");
      search=search.replace(/\\r/g,"\r");
      search=search.replace(/\\t/g,"\t");
      search=search.replace(/\\v/g,"\v");
      search=search.replace(/\\x[0-9a-fA-F]{2}|\\u[0-9a-fA-F]{4}/g,
        function($0,$1,$2){
          return String.fromCharCode(parseInt("0x"+$0.substring(2)));
        }
      );
      search=search.replace(/\\B/g,"\\");
    } else search=search.replace(/\\B/g,"\\\\");
  }
  if (options.indexOf("l")>=0) {
    options=options.replace(/l/g,"");
    search=search.replace(/([.^$*+?()[{\\|])/g,"\\$1");
    if (!jexpr) replace=replace.replace(/\$/g,"$$$$");
  }
  if (options.indexOf("b")>=0) {
    options=options.replace(/b/g,"");
    search="^"+search
  }
  if (options.indexOf("e")>=0) {
    options=options.replace(/e/g,"");
    search=search+"$"
  }
  var search=new RegExp(search,options);
  var str1, str2;

  if (srcVar) {
    str1=env(args.Item(3));
    str2=str1.replace(search,jexpr?replFunc:replace);
    if (!alterations || str1!=str2) if (multi) {
      WScript.Stdout.Write(str2);
    } else {
      WScript.Stdout.WriteLine(str2);
    }
    if (str1!=str2) rtn=0;
  } else if (multi){
    var buf=1024;
    str1="";
    while (!WScript.StdIn.AtEndOfStream) {
      str1+=WScript.StdIn.Read(buf);
      buf*=2
    }
    str2=str1.replace(search,jexpr?replFunc:replace);
    WScript.Stdout.Write(str2);
    if (str1!=str2) rtn=0;
  } else {
    while (!WScript.StdIn.AtEndOfStream) {
      str1=WScript.StdIn.ReadLine();
      str2=str1.replace(search,jexpr?replFunc:replace);
      if (!alterations || str1!=str2) WScript.Stdout.WriteLine(str2);
      if (str1!=str2) rtn=0;
    }
  }
} catch(e) {
  WScript.Stderr.WriteLine("JScript runtime error: "+e.message);
  rtn=3;
}
WScript.Quit(rtn);

function replFunc($0, $1, $2, $3, $4, $5, $6, $7, $8, $9, $10) {
  var $=arguments;
  return(eval(replace));
}
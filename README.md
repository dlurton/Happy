#Happy - A scripting language with syntactic sugar for code generation

## The Short and Sweet:

* Can be used to quickly and efficiently write a generator for any kind of text based output including HTML, XML, CSS, Javascript, C#, VB.Net, SQL, YAML, Java, Markdown, etc, etc.  If it's text-based you can write a program in Happy to generate it.
* Based on the [Dynamic Language Runtime](http://dlr.codeplex.com)
* Fast compilation and execution (can generate NHibernate mappings for an 800+ table database in under 6 seconds).
* Simple Javascript-like syntax with additions for templates
* .NET Interoperability:
	* Instantiate any .NET type 
	* Invoke instance and static methods
	* Read/write properties
* Embeddable in another program 
* Can be executed from the command-line
* Visual Studio Debugging (this is still a WIP but it works!)
* A hand-written lexical analyzer and parser, leading to very fast compilation and small assembly size.

##Future Work:

* try/catch/throw 
* using blocks (a la C#)
* Increment (++) and decrement (--) operators
* Operation assignment operators (+=, -=, *=, /= %=, etc)
* Ternary operator (?:)
* modules (currently, the entire script must be contained in a single file)
* closures (should be easy to implement thanks to the DLR)
* events
* constants
* Visual Studio Debugging needs cleaning up
* Support for changing the template output stream during script execution (so that multiple files may be generated per script run)
* General testing and stabilizing

##The 5-Minute Intro

###Output Statements

Happy has the concept of a "current output."  The current output is simply a `System.IO.TextWriter` where all template output is directed.  At the moment, the current output must be determined by the host program executing the Happy script.  This is usually the command-line application, in which case the current output is directed at the file specified in one of the command-line options.  A host program in which Happy has been embedded can specify any TextReader as the current output.

There are several ways to write to the current output.  The easiest way is using the output operator:

	~"Text to be written to the current output.";

This shows how a single string may be written to the output.  Multiple values may also be written in a single statement as long as they are properly separated by whitespace:

	~"Hello, " username "!";

If the value of the `username` variable was "Bob", this would cause "Hello, Bob!" to be written to the current output.

Also note that any object may be written to the output.  If the object is not a string already, then the `ToString()` method is called to first convert it to a string.

### Templates

In Happy, templates are expressions that have values just like a + b is an expression with a value.  Whenever script exection reaches a template, it is executed immediately and the scripts output is either written to the output or stored in a variable for later use.  Templates begin with `<|` and end with `|>`.  Text between `<|` and `|>` is called verbatim text.  Verbatim text is written to the current output exactly as it appears, new lines and all. Within verbatim text, individual expressions may be written to the output between pairs of `$` .  

To write the output of a template to the current output, use the output operator described above:

	~<|Hello, $nonVerbatimText$! This is some verbatim text!|>;

Assuming `nonVerbatimText` had the value of "Bob", the output would be: 

	Hello, Bob! This is some verbatim text!

Using the same example, we can assign the value of the template to a variable:

	def helloText = <|Hello, $nonVerbatimText$! This is some verbatim text!|>;
	~helloText;

The output is exactly the same as the previous example.

Templates can also contain inline code, similar to ASP/JSP, etc:

	def helloText = <|Hello, |% 
		def userName = getUsername();
		if(String.IsNullOrEmpty(userName)
			userName = "<unknown>";
		~userName;
		%|$! this is some verbatim text!|>;
	~helloText;

If the `getUsername()` function returned null, the output would be:

	Hello, <unknown>! This is some verbatim text!

###A More Complete Example:

	function generateSimpleInvoice(invoice)
	{
		def itemCount = 0;
		~<|
	Invoice Date  $invoice.DateTime$
	Customer:  $invoice.Customer.Name$
	
	Items Purchased:
		|% for(item in invoice.Items) { 
			%|$"\t"$ Description: $item.Description$, Price: $item.Price$ $"\n" |% 
			itemCount = itemCount + 1 
		} %|
			Number of items:  $itemCount$
		|>;
	}

This would output text similar to the following:

	Invoice Date:  10/12/2012 12:43 pm
	Customer:  Acme, Inc
	Items Purchased:
		Description:  Widget A, Price 123
		Description:  Widget B, Price 234
		Description:  Widget C, Price 345
		Number of items:  3

##Syntax Summary

####Comments:

	//This is a comment
	/* 
		this is also a comment 
	*/

####Global variables:

	def foo, bar = "Hello, world!";

(Note the use of "def" instead of "var.")

####Single Line Functions:

	//since the curly braces are optional for if, while and for statements that have only one line
	//why can't functions they also be optional for functions that have only one line?
	function multiply(a, b) 
		return a * b;

####Multi-line functions:

	function foobar(foo, bar)
    {
		def intermidiateValue = someFunction(foo + bar);
		return someOtherFunction(intermediateValue);
    }

####Local variables:

	function example()
	{
		def localFoo, localBar = "Hello, locals";
		//... something with localFoo and localBar
	}

####Branching:

	if(someVariable == anotherVariable)
		Console.WriteLine("Hello, world!");
	else
		Console.WriteLine("Goodbye, world!");

	switch(someValue)
	{
	case "a":
		Console.WriteLine("someValue was 'a'");
		break;
	case "b":
		Console.WriteLine("someValue was 'b');
		break;
	default:
		Console.WriteLine("I don't know what someValue was.");
		break;
	}

####While Loop:

	while(sqlReader.Read())
	{
		def row = readRow(sqlReader);
		if(row.Status == WidgetStatus.Closed)
			break;
	
		if(row.Status == WidgetStatus.Pending)
			continue;
		
		Console.WriteLine("Widget {0} is ready!", row.Id);
	}

####For Loop:

	~"Widgets older than 1 year:"
	for(foo in bar.Widgets where bar.Widget.Age > 1 between "\n\t")
		~foo.WidgetId ": " foo.Description;

The variable "foo" is being declared here, and like a C# `foreach` loop is scoped to the loop body.  Following the keyword `where` is a boolean expression which is evaluated before each iteration--the loop body will only be executed if it evaluates to true.  Following the `between` keyword is an expression whch will be written to the current output *bewtween* iterations, thus solving the "dangling comma" problem (where in logic must exist at the end of a loop body to check if this is the last iteration to prevent a trailing comma, or the trailing comma must be removed after the loop (i.e.:  "1, 2, 3, 4,") and allowing repeated code to be indented at a different level than the code that came before:

	Widgets older than 1 year:
		10: Foo
		12: Bar
		23: Shazam!

Any value may be specified as the between text in a `for` loop.  As with any output expression, if it is not a string, the object.ToString() method is called.

## Object Instantiation:

Object instantiation looks slightly different in Happy than in other languages:

	def newInstance = new(System.DateTime, 2012, 12, 12);

The new keyword looks like a function although it is not actually a function.  The first argument is the type.  The type can be a direct reference to the type in its namespace as in this example or it can be an instance of the System.Type class.  The remaining arguments are passed to the constructor. 

##Using Types in .Net Assemblies

Assemblies may be dynamically "referenced" at runtime by loading them with the `load` keyword:

	load "Microsoft.SqlServer.Smo, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91, processorArchitecture=MSIL"

As in this example, if the assembly is signed with a strong name you must specify the fully qualified assembly name.  If the assembly is not strong named you may simply use the assembly's filename without the extension.

##Using Classes and Namespaces

Indivual types may be placed in the global scope easily by simply assigning them to a global variable:
	
	def Console = System.Console;
	def AnotherNameForConsole = System.Console;

All types in a single namespace may be placed in the global scope easily with the `use` statement, which is analagous to the `using` statement in C#:

	use System;
	use Sysetm.IO;
	use YourCompany.BusinessLayer;

Note that Happy does not handle using namespaces that have types of the same name.  If this situation is encountered, the workaround is to assign the individual classes of one of the namespaces required by the script to global variable names as in the previous example or to use fully qualified type names, i.e. `System.Console` instead of simply `System`.


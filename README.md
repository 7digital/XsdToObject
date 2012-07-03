XsdToObject
===========

Parse a set of XSD documents and produce C# source code.

Features:
* Maps complex XSD types to classes and simple XSD types to System::String
* Maps XSD elements to IList<T> or T depending on value of maxOccurs attribute
* Generated class and property names always starts with upper case letter
* Property names of IList<T> type always ends with 's'

Not implemented:
* The XML attributes are currently not mapped 

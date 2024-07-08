///////////////////////////////////////////////////////////////
//         Project: Call_Augger

// This is a program that will take in 2 sets of data.
// One set of data is a list of phone calls. The other set
// of data is a list Pharmacies. The program will then
// group the phone calls by phone number and if they
// belong to a pharmacy on file it will add that calls
// data to the pharmacy's collection. The program will then
// output a formatted excel file containing several sheets.
// Each sheet will show a different set of data.

///////////////////////////////////////////////////////////////
//	    Input File Formatting

 HEADERS NEEDED for Call Record data:
 Call Type, User Name, Duration, Time, Transfer User, Outbound, To, From

 HEADERS NEEDED for Pharmacy data:
 Pharmacy Name, Contact 1 First Name, Contact 1 Last Name, NPI #, DEA #, NCPDP #, Address, City, State, Zip, Phone # (no dashes)

 these files can be in either CSV or Excel formats, both will be read fine  
 as long as the header row is included. A HEADER ROW IS REQUIRED. 
 the placement of each column doesn’t matter


/*
 * Program:         GradeTracker 
 * File:            Program.cs
 * Date:            May 20, 2021
 * Author:          Vincent Li
 * Description:     This is the main console project that reads from a JSON file and validates it with a schema. 
 *                  It can add/delete courses and add/edit/delete evaluations for the courses. Then it is saved in a JSON file
 */

using System;
using System.IO;                // FileStream class
using Newtonsoft.Json;          // JsonConvert class
using Newtonsoft.Json.Schema;   // JSchema class
using Newtonsoft.Json.Linq;     // JObject class
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Regex class for getting the numbers from strings

namespace GradeTracker
{
    /**
     * Class Name:		Program
     * Purpose:			Used to run the main program and has the additional functions that help the main run as well
     * Coder:			Vincent Li
     * Date:			May 23, 2021
    */
    class Program
    {
        // Constants for the file path names
        private const string JSON_FILE = "course_data.json";
        private const string SCHEMA_FILE = "course_schema.json";

        // storing the JSON files under a folder called JSON
        private const string PROJECT_PATH = "..\\..\\..\\JSON\\";

        private const string JSON_FILE_PATH = PROJECT_PATH + JSON_FILE;
        private const string SCHEMA_FILE_PATH = PROJECT_PATH + SCHEMA_FILE;

        // used for validation
        private static string json_schema;
        

        // this is used to keep track if any changes are made to the json file
        // so in the end it writes all the changes at once instead of calling the write method everytime something changes
        private static bool fileChange = false;     

        // used to hold the course objects
        private static List<Course> courseList;

        static void Main(string[] args)
        {
            // bool for if the JSON file was able to deserialize 
            bool worked = true;

            // used to store the json data
            string json_data;

            // read the schema file from the JSON folder
            if (ReadFile(SCHEMA_FILE_PATH, out json_schema))
            {
                if (!File.Exists(JSON_FILE_PATH))
                {
                    // if it is not found, prompt user to create a new file
                    Console.WriteLine("Grades data file {0} not found. Generating new file...", JSON_FILE);

                    // creating under a folder called "JSON" 
                    // just used to create the file and using the "using" statement so it automatically closes the file 
                    using (StreamWriter sw = new StreamWriter(JSON_FILE_PATH)) {}

                    // initiate the courseList list
                    courseList = new List<Course>();

                    

                    Console.WriteLine("\nFile created.\n");
                }
                // Check if the JSON file already exist and trys to read the data
                else if (File.Exists(JSON_FILE_PATH) && ReadFile(JSON_FILE_PATH, out json_data))
                {
                    try
                    {
                        // this means the file exist, but there is nothing in it
                        if (new FileInfo(JSON_FILE_PATH).Length == 0)
                        {
                            // initiate the courseList list
                            courseList = new List<Course>();
                        }
                        else
                        {
                            // attempt to store the JSON data into the list
                            courseList = JsonConvert.DeserializeObject<List<Course>>(json_data);
                        }

                        IList<string> messages;

                        // If the list is null, then don't validate the data since there is nothing to validate
                        // If it isn't empty/null, validate the data
                        if (courseList?.Any() == true)
                        {
                            // serialize each object and validate it
                            for (int i = 0; i < courseList.Count; i++)
                            {
                                string temp = JsonConvert.SerializeObject(courseList[i]);

                                // If this is true, that means there is invalid data
                                if (!ValidateData(temp, json_schema, out messages))
                                {
                                    Console.WriteLine("Error: invalid data");
                                    worked = false;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("\nError: Failed to convert JSON file.");
                        worked = false; // don't print the main menu if it isn't in the correct format
                    }
                }
                // couldn't read the data
                else
                {
                    Console.WriteLine("\nError: Unable to read {0}", JSON_FILE);
                    worked = false; // don't print the main menu if it doesn't validate
                }

                // if the file read properly:
                if (worked)
                {
                    string status; // used to keep track of the command
                    int temp;
                    do
                    {
                        // display the header
                        Console.WriteLine("\nGrade Tracker\nDeveloped by Vincent Li\n");
                        status = DisplayMainMenu();

                        if (status == "A")
                        {
                            // call the method to add courses
                            AddNewCourse();

                            // clear the screen
                            Console.Clear();
                        }
                        // means it is an index/number. It can't be anything else because the input gets validated in DisplayMainMenu
                        else if (int.TryParse(status, out temp) && temp <= courseList.Count)
                        {
                            // parse the string command to a number and check if it is less or equal to the amount of courses
                            int courseIndex;
                            int.TryParse(status, out courseIndex);

                            // clear the screen
                            Console.Clear();

                            // call the course detail menu
                            status = DisplayCourseDetail(courseIndex);

                            if (status == "A")
                            {
                                // call the method to add evaluations
                                AddNewEvaluation(courseIndex);

                                // clear the screen
                                Console.Clear();
                            }
                            else if (status == "D")
                            {
                                // call the method to delete the course
                                status = DeleteCourse(courseIndex);

                                if (status == "Y")
                                {
                                    Console.Clear();
                                    Console.WriteLine("\nCourse has been deleted.\n");
                                }
                                else if (status == "N")
                                {
                                    Console.Clear();
                                    Console.WriteLine("\nCourse has not been deleted.\n");
                                }
                            }
                            else if (status != "" && status != "B")
                            {
                                int evaluationIndex;
                                // parse the string command if it is a number
                                int.TryParse(status, out evaluationIndex);

                                // clear the screen
                                Console.Clear();

                                // call the evaluation detail menu
                                status = DisplayEvaluationDetail(courseIndex, evaluationIndex);

                                // if status = "B" return to course detail
                                if (status == "B")
                                {
                                    // clear the screen
                                    Console.Clear();
                                }
                                else if (status == "D")
                                {
                                    // delete evaluation
                                    status = DeleteEvaluation(courseIndex, evaluationIndex);

                                    if (status == "Y")
                                    {
                                        Console.Clear();
                                        Console.WriteLine("\nEvaluation has been deleted.\n");
                                    }
                                    else if (status == "N")
                                    {
                                        Console.Clear();
                                        Console.WriteLine("\nEvaluation has not been deleted.\n");
                                    }
                                }
                                else if (status == "E")
                                {
                                    // edit evaluation
                                    EditEvaluation(courseIndex, evaluationIndex);

                                    // clear the screen
                                    Console.Clear();
                                }
                                
                            }
                        } // end if (int...

                    } while (status != "X"); // means exit

                    // check if the file has changed
                    if (fileChange)
                    {
                        // write to file if data has been updated
                        WriteToJSONFile(JSON_FILE_PATH, courseList);
                    }
                }
            }
            else
            {
                Console.WriteLine("\nError: Unable to read/locate {0}", SCHEMA_FILE);
            }

            Console.WriteLine("\nExiting program...");
        } // main

        /*Method Name:  ReadFile
        *Purpose:       Reads all the text from a file and put it into a string variable
        *Accepts:       a path and output string
        *Returns:       boolean for whether or not it worked
        */
        private static bool ReadFile(string path, out string json)
        {
            try
            {
                // used to read JSON file data
                json = File.ReadAllText(path);
                return true;
            }
            catch
            {
                json = null;
                return false;
            }
        } // ReadFile

        /*Method Name:  WriteToJSONFile
        *Purpose:       This is used to write all the data to the JSON file
        *Accepts:       a path and a list of courses
        *Returns:       boolean for whether or not it worked
        */
        private static bool WriteToJSONFile(string path, List<Course> courses)
        {
            // serialize data
            string all_data = JsonConvert.SerializeObject(courses);

            try
            {
                File.WriteAllText(path, all_data);
                Console.WriteLine($"\n\nUpdated json file, data has been written to the JSON folder under {JSON_FILE}.");
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine("\n\n{0}", ex.Message);
                return false;
            }
        } // WriteToJSONFile

        /*Method Name:  ValidateData
        *Purpose:       Used to validate a JSON object against the schema
        *Accepts:       json data, json schema, output for error messages
        *Returns:       boolean for whether or not it worked
        */
        private static bool ValidateData(string json_data, string json_schema, out IList<string> messages)
        {
            JSchema schema = JSchema.Parse(json_schema);
            JObject data = JObject.Parse(json_data);
            return data.IsValid(schema, out messages);
        } // ValidateData

        /*Method Name:  DisplayMainMenu
        *Purpose:       Used to display the main portion of the program (displays the overall view of the courses)
        *Accepts:       Nothing
        *Returns:       String for commands
        */
        public static string DisplayMainMenu()
        {
            Console.WriteLine("\n                             ~ GRADES TRACKING SYSTEM ~\n");
            Console.WriteLine("+--------------------------------------------------------------------------------+");
            Console.WriteLine("|                                 Grade Summary                                  |");
            Console.WriteLine("+--------------------------------------------------------------------------------+");

            // This checks if the list is not null and > 0
            if (courseList?.Count > 0)
            {
                // means there is data so print the courses
                Console.WriteLine("\n#.  Course        Marks Earned        Out of        Percent\n");
                for (int i = 0; i < courseList.Count; i++)
                {
                    double? eval = 0.0;         // the marks earned by the student
                    double outOf = 0.0;         // out of per evaluation
                    double percent = 0.0;       // earned marks / out of per evalation
                    double weight = 0.0;        // the weight of the each evaluation

                    // Under the header name:
                    double courseMarks = 0.0;       // Marks Earned
                    double totalOutOf = 0.0;        // Out of
                    double totalPercent = 0.0;      // Percent

                    // iterate through the evaluations to get the info
                    for (int x = 0; x < courseList[i].Evaluations.Count; x++)
                    {
                        // check if there is any earned marks in the evaluation (for completed courses)
                        if (courseList[i].Evaluations[x].EarnedMarks.HasValue)
                        {
                            // storing the numbers used to calculate the percentage and course marks for each evaluation
                            eval = courseList[i].Evaluations[x].EarnedMarks; // since this is nullable, set it to double ? and with a default value of 0 so if it is null, it will print 0
                            outOf = courseList[i].Evaluations[x].OutOf;
                            weight = courseList[i].Evaluations[x].Weight;

                            // storing total out of marks
                            totalOutOf += courseList[i].Evaluations[x].Weight;

                            // calculate course marks
                            double sum = Convert.ToDouble(eval);
                            percent = 100 * (sum / outOf);          // calculate the percentage per evaluation
                            courseMarks += (percent*weight)/100;    // calculate by taking each percentage of an evaluation * the weight of the evaluation / 100

                        }
                    }

                    // check if totalOutOf is > 0 because you cannot divide by 0
                    if (totalOutOf > 0)
                    {
                        // calculate total percent (course marks / total weight) * 100
                        totalPercent = (courseMarks / totalOutOf) * 100;
                    }

                    // writing the formated string
                    Console.WriteLine("{0}.{1,2}{2,-18}{3,8:0.0}{4,-6}{5,8:0.0}{6,-7}{7,8:0.0}", i + 1,"", courseList[i].Code, courseMarks, "", totalOutOf, "", totalPercent); 
                }
            }
            else
            {
                Console.WriteLine("\nThere are currently no saved courses.");
            }

            // Showing the commands
            Console.WriteLine("\n----------------------------------------------------------------------------------");
            Console.WriteLine("     Press # from the above list view/edit/delete a specific course.");
            Console.WriteLine("     Press A to add a new course.");
            Console.WriteLine("     Press X to quit.");
            Console.WriteLine("----------------------------------------------------------------------------------\n");


            // trying to get valid input
            string input;
            bool validInput = false;

            do
            {
                Console.Write("Enter a command: ");
                input = Console.ReadLine().ToUpper().Trim();

                int index; // used for the course indexing
                // exit
                if (input == "X")
                {
                    validInput = true;
                    
                }
                // adding
                else if (input == "A")
                {
                    validInput = true;

                }
                // If it is a number and the number is <= to the total courses, it is a valid input
                // Also 0 is not a valid input because it is technically not on the list
                else if (int.TryParse(input, out index) && index <= courseList.Count && index != 0)
                {
                    validInput = true;
                }
                else
                {
                    Console.WriteLine("\n\nInvalid command selected. Please try again!\n");
                }

            } while (!validInput);

            return input;

        } // DisplayMainMenu

        /*Method Name:  DisplayCourseDetail
        *Purpose:       This shows the individual course when the index is selected from the list
        *Accepts:       An index for which course it should show
        *Returns:       string for commands
        */
        private static string DisplayCourseDetail(int index)
        {
            // check if it is a valid number 
            if (index > 0 && index <= courseList.Count)
            {
                // access the course
                Course course = courseList[index - 1]; // -1 to account for the extra number added in the main menu for the list

                Console.WriteLine("\n                             ~ GRADES TRACKING SYSTEM ~\n");
                Console.WriteLine("+--------------------------------------------------------------------------------+");
                Console.WriteLine("|                               {0} Evaluations                            |",course.Code); // add the course name thing here
                Console.WriteLine("+--------------------------------------------------------------------------------+");

                // This checks if the list is not null and > 0
                if (course.Evaluations?.Count > 0)
                {
                    // iterate through the evaluations
                    Console.WriteLine("\n#.  Evaluation        Marks Earned   Out of   Percent   Course Marks   Weight/100\n");
                    for (int i = 0; i < course.Evaluations.Count; i++)
                    {
                        double percent = 0.0;
                        double courseMark = 0.0;
                        double? earnedMarks = null;

                        // check if earned marks is null or not
                        if (course.Evaluations[i].EarnedMarks.HasValue)
                        {
                            earnedMarks = course.Evaluations[i].EarnedMarks;

                            double sum = Convert.ToDouble(earnedMarks);
                            // calculate percent
                            percent = 100 * (sum / course.Evaluations[i].OutOf);

                            // calculate course marks
                            courseMark = (percent * course.Evaluations[i].Weight) / 100;
                        }

                        // writing the formated string
                        Console.WriteLine("{0}.{1,2}{2,-23}  {3,5:0.0}    {4,5:0.0}     {5,5:0.0}          {6,5:0.0}        {7,5:0.0}", i + 1, "", course.Evaluations[i].Description, 
                            (earnedMarks.HasValue ? earnedMarks : ""), course.Evaluations[i].OutOf, percent, courseMark, course.Evaluations[i].Weight);
                    }
                }
                else
                {
                    Console.WriteLine("\nThere are currently no evaluations for {0}.",course.Code);
                }

                // Showing commands
                Console.WriteLine("\n----------------------------------------------------------------------------------");
                Console.WriteLine("     Press D to delete this course.");
                Console.WriteLine("     Press A to add a new evaluation.");
                Console.WriteLine("     Press # from the above list edit/delete a specific course.");
                Console.WriteLine("     Press B to return to the main menu.");
                Console.WriteLine("----------------------------------------------------------------------------------\n");

                // trying to get valid input
                string input;
                bool validInput = false;

                do
                {
                    Console.Write("Enter a command: ");
                    input = Console.ReadLine().ToUpper().Trim();

                    int courseIndex; // used for the course indexing
                    
                    // return to main menu
                    if (input == "B")
                    {
                        validInput = true;
                        Console.Clear();
                    }
                    // adding
                    else if (input == "A")
                    {
                        validInput = true;

                    }
                    // delete
                    else if (input == "D")
                    {
                        validInput = true;
                    }
                    // If it is a number and the number is <= to the total evaluations, it is a valid input
                    // Also 0 is not a valid input because it is technically not on the list
                    else if (int.TryParse(input, out courseIndex) && courseIndex <= course.Evaluations.Count && courseIndex != 0)
                    {
                        validInput = true;
                    }
                    else
                    {
                        Console.WriteLine("\n\nInvalid command selected. Please try again!\n");
                    }

                } while (!validInput);

                return input;
            }
            // invalid index
            else
            {
                Console.WriteLine("Error: [{0}] is not a valid index.\n", index);

                return "";
            }
        } // DisplayCourseDetail

        /*Method Name:  DisplayEvaluationDetail
        *Purpose:       Used to display the evaluation details for a specific course
        *Accepts:       course index and evaluation index (for the list)
        *Returns:       string for commands
        */
        private static string DisplayEvaluationDetail(int courseIndex, int evalIndex)
        {
            // check if it is a valid number 
            if (evalIndex > 0 && evalIndex <= courseList[courseIndex-1].Evaluations.Count)
            {
                // access the course
                Course course = courseList[courseIndex - 1]; // -1 to account for the extra number added in the evaluation menu for the list
                evalIndex -= 1;

                Console.WriteLine("\n                             ~ GRADES TRACKING SYSTEM ~\n");
                Console.WriteLine("+--------------------------------------------------------------------------------+");
                Console.WriteLine("                                {0} {1}", course.Code, course.Evaluations[evalIndex].Description);
                Console.WriteLine("+--------------------------------------------------------------------------------+");

                Console.WriteLine("\nMarks Earned   Out Of   Percent   Course Marks   Weight/100\n");

                // check if marks earned exist
                double? marksEarned = null;
                double percent = 0.0;
                double courseMark = 0.0;

                if (course.Evaluations[evalIndex].EarnedMarks.HasValue)
                {
                    marksEarned = course.Evaluations[evalIndex].EarnedMarks;

                    double sum = Convert.ToDouble(marksEarned);

                    // calculate percent
                    percent = 100 * (sum / course.Evaluations[evalIndex].OutOf);

                    // calculate course marks
                    courseMark = (percent * course.Evaluations[evalIndex].Weight) / 100;
                }

                // print data
                Console.WriteLine("{0,-4}{1,8:0.0}    {2,5:0.0}     {3,5:0.0}          {4,5:0.0}        {5,5:0.0}\n", "", 
                    marksEarned.HasValue ? marksEarned : "", course.Evaluations[evalIndex].OutOf, percent, courseMark, course.Evaluations[evalIndex].Weight);

                // Showing the commands
                Console.WriteLine("\n----------------------------------------------------------------------------------");
                Console.WriteLine("     Press D to delete this evaluation.");
                Console.WriteLine("     Press E to edit this evaluation.");
                Console.WriteLine("     Press B to return to the main menu.");
                Console.WriteLine("----------------------------------------------------------------------------------\n");


                // input validation
                string input;
                bool validInput = false;

                do
                {
                    Console.Write("Enter a command: ");
                    input = Console.ReadLine().ToUpper().Trim();

                    // return to previous menu
                    if (input == "B")
                    {
                        validInput = true;
                        Console.Clear();
                    }
                    // editing
                    else if (input == "E")
                    {
                        validInput = true;
                    }
                    // delete
                    else if (input == "D")
                    {
                        validInput = true;
                    }
                    else
                    {
                        Console.WriteLine("\n\nInvalid command selected. Please try again!\n");
                    }

                } while (!validInput);

                return input;
            }
            else
            {
                Console.WriteLine("Error: [{0}] is not a valid index.\n", evalIndex);
                return "";
            }


        } // DisplayEvaluationDetail

        /*Method Name:  AddNewCourse
        *Purpose:       Adds a new course then validates it against the schema when it is added to the list
        *Accepts:       nothing
        *Returns:       nothing
        */
        private static void AddNewCourse()
        {
            Course course = new Course();
            bool valid = false;
            do
            {
                Console.Write("Enter a course code: ");
                course.Code = Console.ReadLine().Trim(); // reading the line and getting rid of any white spaces

                string temp = JsonConvert.SerializeObject(course);
                IList<string> messages;
                valid = ValidateData(temp, json_schema, out messages);
                // check if valid
                if (valid)
                {
                    courseList.Add(course);

                    // this means there is a change to the file
                    fileChange = true;

                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Error: Invalid course data entered. Please try again.\n");
                }

            } while (!valid);
        } // AddNewCourse

        /*Method Name:  AddNewEvaluation
        *Purpose:       Adds a new evaluation for a course
        *Accepts:       An index for the course to access the course data
        *Returns:       nothing
        */
        private static void AddNewEvaluation(int courseIndex)
        {
            bool valid = false;
            Evaluation e = new Evaluation();

            do
            {
                Console.Write("Enter a description: ");
                e.Description = Console.ReadLine().Trim();

                // validate out of mark
                do
                {
                    Console.Write("Enter the 'out of' mark: ");
                    string data = Console.ReadLine();
                    int temp;
                    
                    // using regex in case the input is not numbers since try parse will return 0 if it is not a number
                    valid = int.TryParse(data, out temp) && temp >= 0 && Regex.IsMatch(data, @"-?\d+(?:\.\d+)?");
                    if (valid)
                    {
                        e.OutOf = temp;
                    }
                    else
                    {
                        Console.WriteLine("Error: 'out of' must be >= 0.");
                    }

                } while (!valid);

                // validate for weight
                do
                {
                    Console.Write("Enter the % weight: ");
                    string data = Console.ReadLine();
                    double temp;
                    
                    // using regex in case the input is not numbers since try parse will return 0 if it is not a number
                    valid = double.TryParse(data, out temp) && temp >= 0 && temp <= 100 && Regex.IsMatch(data, @"-?\d+(?:\.\d+)?");

                    if (valid)
                    {
                        e.Weight = temp;
                    }
                    else
                    {
                        Console.WriteLine("Error: 'weight' % must be >= 0 and <= 100.");
                    }

                } while (!valid);
                
                // validate for earned marks
                do
                {
                    Console.Write("Enter marks earned or press ENTER to skip: ");
                    string data = Console.ReadLine();
                    double temp;
                    
                    // using regex in case the input is not numbers since try parse will return 0 if it is not a number
                    valid = (double.TryParse(data, out temp) && Regex.IsMatch(data, @"-?\d+(?:\.\d+)?") && temp >= 0) || data == ""; 

                    if (valid && data != "")
                    {
                        e.EarnedMarks = temp;
                    }
                    else if (data == "")
                    {
                        e.EarnedMarks = null;
                    }
                    else
                    {
                        Console.WriteLine("Error: 'earned marks' must be >= 0 or nothing");
                    }

                } while (!valid);

                courseList[courseIndex-1].Evaluations.Add(e);
                string tempStr = JsonConvert.SerializeObject(courseList[courseIndex-1]);
                IList<string> messages;
                valid = ValidateData(tempStr, json_schema, out messages);

                // check if the data is valid
                if (!valid)
                {
                    courseList[courseIndex - 1].Evaluations.RemoveAt(courseList[courseIndex - 1].Evaluations.Count - 1);
                    Console.WriteLine("Error: data is invalid");
                }
                else
                {
                    fileChange = true;
                }

            } while (!valid);

        } // AddNewEvaluation

        /*Method Name:  DeleteCourse
        *Purpose:       Used to delete a specific course and validate all the data after
        *Accepts:       A course index 
        *Returns:       string for a yes or no to show the message later
        */
        private static string DeleteCourse(int courseIndex)
        {
            bool valid = false;
            string input;
            do
            {
                Console.Write("Delete {0}? (Y/N): ", courseList[courseIndex-1].Code);
                input = Console.ReadLine().ToUpper().Trim();

                if (input == "Y")
                {
                    valid = true;
                    //Console.WriteLine("\nCourse: {0} has been removed.", courseList[courseIndex - 1].Code);
                    courseList.RemoveAt(courseIndex - 1);

                    // validate course list
                    for (int i = 0; i < courseList.Count; i++)
                    {
                        string tempStr = JsonConvert.SerializeObject(courseList[i]);
                        IList<string> messages;
                        valid = ValidateData(tempStr, json_schema, out messages);
                    }
                    // if it is valid, set fileChange to true to write to file at the end
                    if (valid)
                    {
                        fileChange = true;
                    }
                }
                else if (input == "N")
                {
                    valid = true;
                    //Console.WriteLine("Course has not been deleted.");
                }
                else
                {
                    Console.WriteLine("Error: Invalid entry.");
                }
            } while (!valid);

            return input;
        } // DeleteCourse

        /*Method Name:  DeleteEvaluation
        *Purpose:       Deletes an evaluation for a course
        *Accepts:       course index and evaluation index (to access the course list data)
        *Returns:       string for a yes or no for the output message
        */
        private static string DeleteEvaluation(int courseIndex, int evalIndex)
        {
            bool valid = false;
            string input;
            do
            {
                Console.Write("Delete {0}? (Y/N): ", courseList[courseIndex - 1].Evaluations[evalIndex-1].Description);
                input = Console.ReadLine().ToUpper().Trim();

                // delete if they said yes
                if (input == "Y")
                {
                    valid = true;
                    //Console.WriteLine("\nEvaluation: {0} has been deleted from {1}.", courseList[courseIndex - 1].Evaluations[evalIndex - 1].Description, courseList[courseIndex - 1].Code);
                    courseList[courseIndex-1].Evaluations.RemoveAt(evalIndex - 1);

                    // validate course list
                    for (int i = 0; i < courseList.Count; i++)
                    {
                        string tempStr = JsonConvert.SerializeObject(courseList[i]);
                        IList<string> messages;
                        valid = ValidateData(tempStr, json_schema, out messages);
                    }
                    // if it is valid, set fileChange to true to write to file at the end
                    if (valid)
                    {
                        fileChange = true;
                    }
                }
                else if (input == "N")
                {
                    valid = true;
                    //Console.WriteLine("Evaluation has not been deleted.");
                }
                else
                {
                    Console.WriteLine("Error: Invalid entry.");
                }
            } while (!valid);

            return input;
        } // DeleteEvaluation

        /*Method Name:  EditEvaluation
        *Purpose:       Edits the 'earned marks' for the evaluation
        *Accepts:       Course index and evaluation index to access the list
        *Returns:       nothing
        */
        private static void EditEvaluation(int courseIndex, int evalIndex)
        {
            bool valid;
            // check if it is a valid index
            if (evalIndex > 0 && evalIndex <= courseList[courseIndex - 1].Evaluations.Count)
            {
                evalIndex -= 1; // setting it to its proper index

                // check if the input is valid
                do
                {
                    Console.Write("Enter the marks earned out of {0}, press ENTER to leave unassigned: ", courseList[courseIndex-1].Evaluations[evalIndex].OutOf);
                    string data = Console.ReadLine();
                    double temp;

                    // checking if it is a double or "" for the null aspect
                    // using regex in case the input is not numbers since try parse will return 0 if it is not a number
                    // also checking range (since there is no max range for earned marks, just go from 0 to whatever
                    valid = (double.TryParse(data, out temp) && Regex.IsMatch(data, @"-?\d+(?:\.\d+)?") && temp >= 0) || data == "";

                    if (valid && data != "")
                    {
                        courseList[courseIndex-1].Evaluations[evalIndex].EarnedMarks = temp;
                    }
                    else if (valid && data == "")
                    {
                        // don't change anything if nothing is there
                    }
                    else
                    {
                        Console.WriteLine("Error: 'earned marks' must be >= 0, or nothing");
                    }

                } while (!valid);

                // validate
                string tempStr = JsonConvert.SerializeObject(courseList[courseIndex - 1]);
                IList<string> messages;
                valid = ValidateData(tempStr, json_schema, out messages);
                // checking it with the schema
                if (valid)
                {
                    // add changes
                    fileChange = true;
                }
                else
                {
                    // change was invalid, remove it from list
                    courseList[courseIndex - 1].Evaluations.RemoveAt(evalIndex);
                }
            }
            else
            {
                Console.WriteLine("Error: [{0}] is not a valid index.\n", evalIndex);
            }

        } // EditEvaluation
    }
}

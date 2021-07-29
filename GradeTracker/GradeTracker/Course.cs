/*
 * Program:         GradeTracker 
 * File:            Program.cs
 * Date:            May 20, 2021
 * Author:          Vincent Li
 * Description:     This is used to store the course data from the JSON
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeTracker
{
    /**
	 * Class Name:		Course
	 * Purpose:			Used to store the Course object from the JSON file
	 * Coder:			Vincent Li
	 * Date:			May 23, 2021
    */
    class Course
    {
        public Course() { Evaluations = new List<Evaluation>(); }
        public string Code { get; set; }

        public List<Evaluation> Evaluations; // optional

    }
}

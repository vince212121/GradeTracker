/*
 * Program:         GradeTracker 
 * File:            Program.cs
 * Date:            May 20, 2021
 * Author:          Vincent Li
 * Description:     This is used to store the evaluation portion of the JSON file
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeTracker
{
    /**
	 * Class Name:		Evaluation
	 * Purpose:			Used to store the evaluation object array from the JSON file
	 * Coder:			Vincent Li
	 * Date:			May 23, 2021
    */
class Evaluation
    {
        public string Description { get; set; }
        public double Weight { get; set; }
        public int OutOf { get; set; }
        public double ? EarnedMarks { get; set; } // nullable
    }
}

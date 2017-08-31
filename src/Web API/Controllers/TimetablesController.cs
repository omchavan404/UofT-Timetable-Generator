﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UoftTimetableGenerator.DataModels;
using UoftTimetableGenerator.Generator;
using UoftTimetableGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Cors;

namespace UoftTimetableGenerator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    [Produces("application/json")]
    public class TimetablesController : Controller
    {
        // PUT api/timetables
        [HttpPut]
        [Route("GetUoftTimetables")]
        public IActionResult GetUoftTimetables([FromBody] TimetableRequest request)
        {
            // Each course code has a length of 10; max 10 courses to put in timetable
            if (request.Courses == null || request.Courses.Length > 100)
                return BadRequest();

            // Check if the preferences / restrictions are set
            if (request.Preferences == null || request.Restrictions == null)
                return BadRequest();

            // Get the courses from the database
            List<Course> courseObjs = new List<Course>();
            foreach (Course course in request.Courses)
            {
                string code = course.CourseCode;
                Course courseObj = UoftDatabaseService.GetCourseDetails(code);
                if (courseObj == null)
                    return NotFound();
                courseObjs.Add(courseObj);
            }

            // Generate the timetables
            GAGenerator generator = new GAGenerator(courseObjs, request.Preferences, request.Restrictions)
            {
                NumGenerations = 100,
                PopulationSize = 16,
                MutationRate = 0.01,
                CrossoverRate = 0.9,
                CrossoverType = "Uniform Crossover"
            };

            List<YearlyTimetable> timetables = generator.GetTimetables();

            // Convert the timetables to mini timetables (which will be presented to the user)
            List<SimplifiedYearlyTimetable> miniTimetables = new List<SimplifiedYearlyTimetable>();
            for (int i = 0; i < timetables.Count; i++)
                miniTimetables.Add(new SimplifiedYearlyTimetable(timetables[i], "Timetable #" + (i + 1)));                

            return Created("api/timetables/getuofttimetables", miniTimetables);
        }
    }
}

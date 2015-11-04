﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using SolutionMerger.Models;
using SolutionMerger.Utils;
using FileRecurse;

namespace SolutionMerger.Parsers
{
    public class SolutionInfo
    {
        public string Name { get; private set; }
        public string BaseDir { get; private set; }
        public string Text { get; private set; }

        public static SolutionInfo Parse(string slnPath)
        {
            var slnText = File.ReadAllText(slnPath);
            var path = Path.GetFullPath(slnPath);
            var slnBaseDir = Path.GetDirectoryName(path);
            var props = SolutionPropertiesInfo.Parse(slnText);

            var sln =  new SolutionInfo(Path.GetFileNameWithoutExtension(path), slnBaseDir, props, new NestedProjectsInfo()) { Text = slnText };
            sln.Projects = ProjectInfo.Parse(sln);

            return sln;
        }

        private SolutionInfo CreateNestedDirs()
        {
            Project.GenerateProjectDirs(NestedSection, Projects);
            return this;
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(BaseDir, Name + ".sln"), ToString());
        }

        private SolutionInfo(string name, string baseDir, SolutionPropertiesInfo propsSection, NestedProjectsInfo nestedSection)
        {
            Name = name;
            BaseDir = Path.GetFullPath(baseDir);
            NestedSection = nestedSection;
            PropsSection = propsSection;
        }

        public override string ToString()
        {
            return string.Format(
                @"Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010{0}
Global
{1}
{2}
EndGlobal
", string.Concat(Projects.Select(p => p.ProjectInfo)), PropsSection, NestedSection);
        }

        public List<BaseProject> Projects { get; private set; }
        private SolutionPropertiesInfo PropsSection { get; set; }
        private NestedProjectsInfo NestedSection { get; set; }

        public static SolutionInfo MergeSolutions(string newName, string baseDir, out string warnings, params SolutionInfo[] solutions)
        {
            var allProjects = solutions.SelectMany(s => s.Projects).Distinct(BaseProject.ProjectGuidLocationComparer).ToList();

            warnings = SolutionDiagnostics.DiagnoseDupeGuids(solutions);

            var mergedSln = new SolutionInfo(newName, baseDir, solutions[0].PropsSection, new NestedProjectsInfo()) { Projects = allProjects };
            mergedSln.CreateNestedDirs()
                .Projects.ForEach(pr => pr.ProjectInfo.SolutionInfo = mergedSln);

            return mergedSln;
        }

        public static SolutionInfo ConsolidateSolutions(string newName, string baseDir, out string warnings, params SolutionInfo[] solutions) {
            var allProjects = solutions.SelectMany(s => s.Projects).Distinct(BaseProject.ProjectGuidLocationComparer).ToList();

            warnings = SolutionDiagnostics.DiagnoseDupeGuids(solutions);

            var scattered = from solution in solutions where solution.BaseDir != baseDir select solution;
            var cleanedList = (from solution in solutions where solution.BaseDir == baseDir select solution).ToList();

            //base directory is output directory
            //copy everything from solutions which aren't in the base directory into the base directory
            foreach (var slnInfo in scattered) {
                var source = slnInfo.BaseDir;
                var dest = baseDir;

                FileUtil.RecursiveFileCopy(source, dest);

                //add new items to cleaned list
                SolutionInfo copiedSln = SolutionInfo.Parse(dest + "\\" + slnInfo.Name + ".sln");
                cleanedList.Add(copiedSln);

                //remove the old sln file which was copied
                File.Delete(baseDir + "\\" + slnInfo.Name + ".sln");
            }

            return MergeSolutions(newName, baseDir, out warnings, cleanedList.ToArray());
        }
    }
}
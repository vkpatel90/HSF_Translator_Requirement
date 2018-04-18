// Copyright (c) 2018 California Polytechnic State University
// Authors: Viren Patel (patel.virenk@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using HSFSystem;
using UserModel;
using MissionElements;
using log4net;
using System.Threading.Tasks;
using IronPython.Hosting;
using HSFUniverse;
using System.Xml;
using Microsoft.Scripting;

namespace HSFScheduler
{

    [Serializable]
    public class Requirement
    {
        #region Attributes
        protected dynamic _pythonInstance;

        //Include attriubutes for: parameter name, comparison operator, value. (and python function?)
        public string ReqName, ReqCompStr, ReqCompValStr;

        #endregion

        #region Constructors

        public struct ReqStrings
        {
            public string Name;
            public string CompareStr;
            public string CompareValStr;
        }
        //
        
        ///*
        public Requirement()
        {

            
            return;
 
        }


        #endregion

        #region Methods
        ReqStrings ReqStringFetch(XmlNode reqXMLNode)
        {
            ReqStrings values = new ReqStrings();

            if (reqXMLNode.Attributes["name"] != null)
            {
                values.Name = reqXMLNode.Attributes["name"].Value.ToString().ToLower();
                values.CompareStr = reqXMLNode.Attributes["type"].Value.ToString().ToLower();
                values.CompareValStr = reqXMLNode.Attributes["value"].Value.ToString().ToLower();
            }
            else if (reqXMLNode.Attributes["type"] != null)
                values.CompareStr = reqXMLNode.Attributes["type"].Value.ToString().ToLower();
            else
                throw new MissingMemberException("Missing a name or type field for requirement!");
            return values;

        }

        public void AnalyzeRequirement(XmlNode requirementXMLNode, int num_asset)
        {

            ReqStrings ReqVal = ReqStringFetch(requirementXMLNode);

            ReqName = ReqVal.Name;
            ReqCompStr = ReqVal.CompareStr;
            ReqCompValStr = ReqVal.CompareValStr;

            string pythonFilePath = string.Empty;
            pythonFilePath = (@"..\\..\\..\\" + ReqName + ".py"); //Append file path name ("..\..\..\" + name + ".py")
            Console.WriteLine(pythonFilePath);


            var engine = Python.CreateEngine();
            var p = engine.GetSearchPaths();
            p.Add(@"C:\Python27\Lib");
            engine.SetSearchPaths(p);
            var scope = engine.CreateScope();
            var ops = engine.Operations;



            dynamic pypy = engine.ExecuteFile(pythonFilePath); //Can we put this into a separate funtion to call out?
            dynamic reqInst;


            //Cannot execute string python file name designation because C# is not a dynamic language in that sense.
            if (ReqName == "imgcapqty")
            {
                reqInst = pypy.imgcapqty(); //THIS IS WHERE IT'S HARDCODED? HOW DO I FIX THIS?
            }
            else if (ReqName == "datalat")
            { 
                reqInst = pypy.datalat(); //THIS IS WHERE IT'S HARDCODED? HOW DO I FIX THIS?
            }
            else
                throw new MissingMemberException("Requirement analysis python function not found!");

            reqInst.main(num_asset, ReqCompStr, ReqCompValStr); //Run Python Code

            return;
        }

        #endregion

    }
}

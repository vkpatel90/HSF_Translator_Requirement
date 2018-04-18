// Copyright (c) 2018 California Polytechnic State University
// Authors: Viren Patel (patel.virenk@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFSystem;
using UserModel;
using MissionElements;
using log4net;
using IronPython.Hosting;
using HSFUniverse;
using System.Xml;
using Microsoft.Scripting;

namespace HSFScheduler
{

    [Serializable]
    public class HSFTranslator
    {
        #region Constructors
        public HSFTranslator()
        {


            return;

        }
        #endregion

        #region Methods

        string GetXMLParamVal(XmlNode ParentNode)
        {

            string nodeItemVal = "";

            foreach (XmlNode ndxNode in ParentNode.ChildNodes)
            {
                if (ndxNode.Name.Equals("defaultValue"))
                {
                    nodeItemVal = ndxNode.Attributes["value"].Value;
                    //Save Param Value
                }
            }

            return nodeItemVal;

        }

        public bool SysMLCheck(string inputFile)
        {
            bool isSysMLBool = false; //Assume not a SysML file until proven otherwise.
            var SysMLDocCheck = new XmlDocument();
            SysMLDocCheck.Load(inputFile);
            //Check if file is from SysML through <packageImport> tag.
            if (SysMLDocCheck.SelectSingleNode("//packageImport") != null)
            {
                isSysMLBool = true;
            }
                return isSysMLBool;
        }

        public string TranslateSysML(string SysMLFile, string SysMLFilePath)
        {
            //Fetch the XML File
            //string modelInputFilePath will the input for this code
            //string SysMLFile = @"..\..\..\HSF_Aeolus_Model.xml";

            HSFTranslator hsftranslator = new HSFTranslator();
            //hsftranslator.LoadSubsystems(SysMLFile);

            var SysMLDoc = new XmlDocument();
            SysMLDoc.Load(SysMLFile);
            XmlNodeList modelXMLNodeList = SysMLDoc.GetElementsByTagName("packagedElement");
            var XmlEnum = modelXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            XmlEnum.MoveNext();
            XmlNode remainingNode = (XmlNode)XmlEnum.Current;

            string lvl1_tag, lvl2_tag, lvl3_tag;
            string paramTag = "", paramVal = "";
            string assetParam = "", assetName = "";
            string dynStateTag = "";
            string subSysParamName = "", subSysParamVal = "";

            string icParamName = "", icParamVal = "";
            string depParamName = "", depParamVal = "";
            string conParamName = "", conParamVal = "";
            string statevarTag = "", stateVarName = "", stateVarVal = "";
            string reqParamName = "", reqParamVal = "";



            //NEXT STEP: CREATE XML FILE
            //REFERENCE
            //http://stackoverflow.com/questions/4094940/c-sharp-create-simple-xml-file

            XmlDocument doc = new XmlDocument();
            XmlNode docNodeXML = doc.CreateXmlDeclaration("1.0", null, null); //CREATE HEADER NODE
            doc.AppendChild(docNodeXML);
            XmlNode modelNodeXML = doc.CreateElement("MODEL"); //CREATE MODEL NODE

            //We're now at the level with PYTHON, ASSET, ASSET, & REQUIREMENT
            foreach (XmlNode modelChildNode in remainingNode.ChildNodes)
            {
                if (modelChildNode.Name.Equals("nestedClassifier"))
                {
                    lvl1_tag = modelChildNode.Attributes["name"].Value; //Level 1 Name

                    if (lvl1_tag == "PYTHON")
                    {
                        //CAN THIS BE SIMPLIFIED TO A SINGLE METHOD?
                        XmlNode pyNodeXML = doc.CreateElement("PYTHON"); //CREATE ENVIRONMENT NODE

                        foreach (XmlNode pythonNode in modelChildNode.ChildNodes)
                        {
                            paramTag = pythonNode.Attributes["name"].Value; //Save Parameter Name
                            paramVal = hsftranslator.GetXMLParamVal(pythonNode); //Save Parameter Value

                            XmlAttribute pyAttribute = doc.CreateAttribute(paramTag);
                            pyAttribute.Value = paramVal;
                            pyNodeXML.Attributes.Append(pyAttribute); //Save PYTHON Parameter Set

                        }

                        modelNodeXML.AppendChild(pyNodeXML); //Append PYTHON Tag
                    }

                    if (lvl1_tag == "ASSET")
                    {
                        //CAN THIS BE SIMPLIFIED TO A SINGLE METHOD?
                        XmlNode assetNodeXML = doc.CreateElement("ASSET"); //CREATE ENVIRONMENT NODE

                        foreach (XmlNode subConNode in modelChildNode.ChildNodes)
                        {

                            lvl2_tag = subConNode.Attributes["name"].Value;

                            if (subConNode.Name.Equals("ownedAttribute"))
                            {
                                //Save Asset Name
                                assetParam = subConNode.Attributes["name"].Value;
                                assetName = hsftranslator.GetXMLParamVal(subConNode);

                                XmlAttribute assetAttribute = doc.CreateAttribute(assetParam);
                                assetAttribute.Value = assetName;
                                assetNodeXML.Attributes.Append(assetAttribute); //Save ASSET Parameter Set
                            }

                            if (subConNode.Name.Equals("nestedClassifier"))
                            {
                                if (lvl2_tag == "DynamicState")
                                {
                                    XmlNode dynSNodeXML = doc.CreateElement("DynamicState"); //CREATE DYNAMIC STATE NODE

                                    foreach (XmlNode dynStateNode in subConNode.ChildNodes)
                                    {
                                        if (dynStateNode.Name.Equals("ownedAttribute"))
                                        {
                                            paramTag = dynStateNode.Attributes["name"].Value; //Save Param Name
                                            paramVal = hsftranslator.GetXMLParamVal(dynStateNode); //Save Param Value

                                            XmlAttribute dynSAttribute = doc.CreateAttribute(paramTag);
                                            dynSAttribute.Value = paramVal;
                                            dynSNodeXML.Attributes.Append(dynSAttribute); //Save DynamicState Parameter Set
                                        }

                                        if (dynStateNode.Name.Equals("nestedClassifier"))
                                        {

                                            dynStateTag = dynStateNode.Attributes["name"].Value;

                                            if (dynStateTag == "EOMS")
                                            {

                                                XmlNode EOMSNodeXML = doc.CreateElement("EOMS"); //CREATE EOMS NODE

                                                foreach (XmlNode EOMNode in dynStateNode.ChildNodes)
                                                {
                                                    if (EOMNode.Name.Equals("ownedAttribute"))
                                                    {
                                                        paramTag = EOMNode.Attributes["name"].Value; //Save Param Name
                                                        paramVal = hsftranslator.GetXMLParamVal(EOMNode); //Save Param Value

                                                        XmlAttribute EOMAttribute = doc.CreateAttribute(paramTag);
                                                        EOMAttribute.Value = paramVal;
                                                        EOMSNodeXML.Attributes.Append(EOMAttribute); //Save EOMS Parameter Set

                                                    }
                                                }

                                                dynSNodeXML.AppendChild(EOMSNodeXML); //Append EOMS

                                            }


                                        }
                                    }

                                    assetNodeXML.AppendChild(dynSNodeXML); // Append DynamicState
                                }

                                //Continue with Subsystem Organization
                                //Create sub method to simplify repeating portions of code logic

                                if (lvl2_tag == "SUBSYSTEM")
                                {

                                    XmlNode subNodeXML = doc.CreateElement("SUBSYSTEM"); //CREATE SUBSYSTEM NODE

                                    //Open Subsystem Tag
                                    foreach (XmlNode subSysNode in subConNode.ChildNodes)
                                    {
                                        if (subSysNode.Name.Equals("ownedAttribute"))
                                        {
                                            subSysParamName = subSysNode.Attributes["name"].Value;
                                            subSysParamVal = hsftranslator.GetXMLParamVal(subSysNode);

                                            //Save Param Set
                                            XmlAttribute subAttribute = doc.CreateAttribute(subSysParamName);
                                            subAttribute.Value = subSysParamVal;
                                            subNodeXML.Attributes.Append(subAttribute); //Save Subsystem Parameter Set
                                        }

                                        if (subSysNode.Name.Equals("nestedClassifier"))
                                        {


                                            lvl3_tag = subSysNode.Attributes["name"].Value;

                                            if (lvl3_tag == "IC")
                                            {

                                                XmlNode ICNodeXML = doc.CreateElement("IC"); //CREATE SUBSYSTEM NODE

                                                //Open IC Tag
                                                foreach (XmlNode icNode in subSysNode.ChildNodes)
                                                {
                                                    icParamName = icNode.Attributes["name"].Value;
                                                    icParamVal = hsftranslator.GetXMLParamVal(icNode);
                                                    //Save IC Param Set
                                                    XmlAttribute ICAttribute = doc.CreateAttribute(icParamName);
                                                    ICAttribute.Value = icParamVal;
                                                    ICNodeXML.Attributes.Append(ICAttribute); //Save Subsystem Parameter Set

                                                }

                                                subNodeXML.AppendChild(ICNodeXML);//Close IC Tag
                                            }

                                            if (lvl3_tag == "DEPENDENCY")
                                            {
                                                XmlNode depNodeXML = doc.CreateElement("DEPENDENCY"); //CREATE DEPENDENCY NODE

                                                foreach (XmlNode depNode in subSysNode.ChildNodes)
                                                {
                                                    depParamName = depNode.Attributes["name"].Value;
                                                    depParamVal = hsftranslator.GetXMLParamVal(depNode);
                                                    //Save Dep Param Set
                                                    XmlAttribute depAttribute = doc.CreateAttribute(depParamName);
                                                    depAttribute.Value = depParamVal;
                                                    depNodeXML.Attributes.Append(depAttribute); //Save Subsystem Parameter Set
                                                }

                                                subNodeXML.AppendChild(depNodeXML); //Close Dep Tag
                                            }
                                        }

                                        //Close Subsystem Tag
                                        assetNodeXML.AppendChild(subNodeXML);
                                    }
                                }

                                if (lvl2_tag == "CONSTRAINT")
                                {
                                    //Open CONSTRAINT Tag
                                    XmlNode conNodeXML = doc.CreateElement("CONSTRAINT"); //CREATE DEPENDENCY NODE

                                    foreach (XmlNode conNode in subConNode.ChildNodes)
                                    {
                                        if (conNode.Name.Equals("ownedAttribute"))
                                        {
                                            conParamName = conNode.Attributes["name"].Value;
                                            conParamVal = hsftranslator.GetXMLParamVal(conNode);
                                            //Save CONSTRAINT Param Set
                                            XmlAttribute conAttribute = doc.CreateAttribute(conParamName);
                                            conAttribute.Value = conParamVal;
                                            conNodeXML.Attributes.Append(conAttribute); //Save Subsystem Parameter Set
                                        }

                                        if (conNode.Name.Equals("nestedClassifier"))
                                        {
                                            statevarTag = conNode.Attributes["name"].Value;

                                            if (statevarTag == "STATEVAR")
                                            {
                                                XmlNode statevarNodeXML = doc.CreateElement("STATEVAR"); //CREATE DEPENDENCY NODE
                                                //Open STATEVAR Tag

                                                foreach (XmlNode stateVarNode in conNode)
                                                {
                                                    stateVarName = stateVarNode.Attributes["name"].Value;
                                                    stateVarVal = hsftranslator.GetXMLParamVal(stateVarNode);
                                                    //Save STATEVAR Set
                                                    XmlAttribute statAttribute = doc.CreateAttribute(stateVarName);
                                                    statAttribute.Value = stateVarVal;
                                                    statevarNodeXML.Attributes.Append(statAttribute); //Save Subsystem Parameter Set
                                                }
                                                //Close STATEVAR Tag
                                                conNodeXML.AppendChild(statevarNodeXML); //Close Dep Tag
                                            }

                                        }
                                    }
                                    //Close CONSTRAINT TAG
                                    assetNodeXML.AppendChild(conNodeXML); //Close Dep Tag
                                }

                            }

                        }

                        modelNodeXML.AppendChild(assetNodeXML); //Append ASSET Tag
                    }

                    if (lvl1_tag == "REQUIREMENT")
                    {
                        //Open REQUIREMENT Tag
                        //CAN THIS BE SIMPLIFIED TO A SINGLE METHOD?
                        XmlNode requireNodeXML = doc.CreateElement("REQUIREMENT"); //CREATE REQUIREMENT NODE

                        foreach (XmlNode reqNode in modelChildNode.ChildNodes)
                        {
                            reqParamName = reqNode.Attributes["name"].Value;
                            reqParamVal = hsftranslator.GetXMLParamVal(reqNode);
                            //Save Requirement Set
                            XmlAttribute reqAttribute = doc.CreateAttribute(reqParamName);
                            reqAttribute.Value = reqParamVal;
                            requireNodeXML.Attributes.Append(reqAttribute); //Save Subsystem Parameter Set

                        }

                        //Close REQUIREMENT Tag
                        modelNodeXML.AppendChild(requireNodeXML); //Append REQUIREMENT Tag
                    }

                }

            }

            //Append All & Save the File
            doc.AppendChild(modelNodeXML);
            SysMLFilePath = Path.GetFullPath(Path.Combine(SysMLFilePath, @"..\..\..\"));
            doc.Save(SysMLFilePath + @"\new_HSF_file.xml");

            string HSFdocName = @"..\..\..\new_HSF_file.xml";
            
            return HSFdocName;

        }

        #endregion
    }
}
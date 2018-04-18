import sys
import os
import operator
import clr
import csv
import System.Collections.Generic
import System

clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')
clr.AddReferenceByName('HSFUniverse')
clr.AddReferenceByName('UserModel')
clr.AddReferenceByName('MissionElements')
clr.AddReferenceByName('HSFSystem')

import System.Xml
import HSFSystem
import HSFSubsystem
import MissionElements
import Utilities
import HSFUniverse
import UserModel
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

sys.path.append(r"C:\Program Files\IronPython 2.7\Lib")

class imgcapqty():
    def __init__(self):
        pass
    def main(self, asset_num, comp_arg, comp_val_str):  # MAIN FUNCTION DECLARATION
        comp_val = float(comp_val_str)
        # INPUTS FOR THIS FUNCTION (All defaulted for now)
        # variable = numingcapture
        # asset_num = 2 # number of assets
        # comp_arg = "GREATER_THAN"
        # comp_val = 10 # comparison value

        # text_file = open("imgcapreq.txt", "w")
        cwd = os.getcwd()
        #print(cwd)
        path = "C:/HorizonLog/Scratch"  # Data File Path per HSF
        os.chdir(path)  # Change Directory

        text_file = open("imgcapreq.txt", "w")

        # Establish operator designations.
        ops = {"<": operator.lt, ">": operator.gt, "<=": operator.le, ">=": operator.ge, "==": operator.eq,
               "!=": operator.ne}

        for index in range(asset_num):  # For each asset...
            # number of images captured data file name
            csv_file = "asset%d_numimgcapture.csv" % (index + 1)

            result_data = []  # Empty array to import data
            with open(csv_file) as csvdata:  # Read csv data into array
                reader = csv.reader(csvdata, delimiter=",")
                for row in reader:
                    result_data.append(row)
            total = 0
            ind = len(result_data)  # Length of array (number of data points)

            for ndx in range(1, ind):
                part1 = result_data[ndx]  # Select Data Point
                part2 = int(part1[1])  # Only Image Captured Boolean is needed
                if part2 == 1:  # If Image Captured == True
                    total = total + 1  # Add 1 to total count of captured images

            # Determine Comparison Operator
            if comp_arg == "less_than":
                arg = "<"
            elif comp_arg == "greater_than":
                arg = ">"
            elif comp_arg == "less_than_or_equal":
                arg = "<="
            elif comp_arg == "greater_than_or_equal":
                arg = ">="
            elif comp_arg == "equal":
                arg = "=="
            elif comp_arg == "not_equal":
                arg = "!="
            else:
                arg = "INCORRECT"


            asset_disp_num = "ASSET %d" % (index + 1)  # For This Asset the comparison is...
            total_cap = "%d images captured" % total

            if arg == "INCORRECT":  # Invalid
                output1 = "ERROR: INVALID COMPARISON ARGUMENT"
                output2 = "PLEASE MAKE SURE COMPARISON OPERATOR IS VALID"
            else:
                arg_check = ops[arg](total, comp_val)  # Evaluate Comparison Operator
                if arg_check:  # True
                    output1 = "SUCCESS: SYSTEM-LEVEL REQUIREMENTS HAVE BEEN MET:"
                    output2 = "%d %s %d" % (total, arg, comp_val)
                else:  # False
                    output1 = "FAILURE: SYSTEM-LEVEL REQUIREMENTS HAVE NOT BEEN MET:"
                    output2 = "%d is not %s %d" % (total, arg, comp_val)

            # Print Results
            text_file.write(asset_disp_num)
            text_file.write("\n")
            text_file.write(total_cap)
            text_file.write("\n")
            text_file.write(output1)
            text_file.write("\n")
            text_file.write(output2)
            text_file.write("\n")

        text_file.close()
        os.chdir(cwd) # Change Directory
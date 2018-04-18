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

#INSTRUCTIONS:
#STEP 1: File > Save As... > (Enter Requirement Analysis Class Name) > Save
#STEP 2: Replace "req_template" with Requirement Analysis Class Name (Same as file name) - Lines ## & ##
#STEP 3: Add any relevant code necessary for requirement analysis after the "total" variable is set as 0.
#STEP 4: Edit/Add any other parts in the code to ensure all relevant data is fetched and analysis is conducted correctly.
#STEP 6: Make sure that HSF is creating the state variable data files you need to codunct your analysis.
# If not, you will need to add state varilables to the HSF code base to calculate/record the necessary state variables.

class req_template(): #Replace "req_template" with Requirement Analysis Class Name
    def __init__(self): #DO NOT CHANGE
        pass
    def main(self, asset_num, comp_arg, comp_val_str):  # MAIN FUNCTION DECLARATION. DO NOT CHANGE
        comp_val = float(comp_val_str) #DO NOT CHANGE
        # INPUTS FOR THIS FUNCTION
        # asset_num = number of assets (integer)
        # comp_arg = comparison argument (string)
        # comp_val = comparison value (double)
        text_file = open("req_template.txt", "w") #Replace "req_template" with Requirement Analysis Class Name
        cwd = os.getcwd()
        #print(cwd)
        path = "C:/HorizonLog/Scratch"  # Data File Path per HSF
        os.chdir(path)  # Change Directory

        # Operator Designations. DO NOT CHANGE
        ops = {"<": operator.lt, ">": operator.gt, "<=": operator.le, ">=": operator.ge, "==": operator.eq,
               "!=": operator.ne}

        for index in range(asset_num):  # For each asset...

            # THE FOLLOWING 14 LINES CAN BE COPIED AS NEEDED TO FETCH OTHER STATE VARIABLE DATA. RENAME VARIABLES AS NEEDED.
            csv_file = "asset%d_XXXXXX.csv" % (index + 1) #Replace XXXXXX with state variable name needed for analysis
            raw_data1 = [] # Empty array to import csv
            data_real1 = [] # Empty array to import state variable data
            data_time1 = [] # Empty array to import time data
            with open(csv_file) as csvdata:  # Read csv data into array
                reader = csv.reader(csvdata, delimiter=",")
                for row in reader:
                    raw_data1.append(row)
            ind1 = len(raw_data1)  # Length of array (number of data points)

            for ndx in range(1, ind1):
                part1 = raw_data1[ndx]  # Select Data Point
                data_real1.append(float(part1[1]))
                data_time1.append(float(part1[0])) # Only necessary if time values are needed in calculations.

            total = 0
            # INSERT ANALYSIS CODE HERE
            # This is where you can manipulate the fetched raw data to formulate an analysis value (total).


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

            #Result Strings
            asset_disp_num = "ASSET %d" % (index + 1)  # For This Asset the comparison is...
            total_str = "%f units" % total #This string can be editted as needed to convey the calculated requirement value.

            #CHECK RESULTS. DO NOT CHANGE
            if arg == "INCORRECT":  # Invalid
                output1 = "ERROR: INVALID COMPARISON ARGUMENT"
                output2 = "PLEASE MAKE SURE COMPARISON OPERATOR IS VALID"
            else:
                arg_check = ops[arg](total, comp_val)  # Evaluate Comparison Operator
                if arg_check:  # True
                    output1 = "SUCCESS: SYSTEM-LEVEL REQUIREMENTS HAVE BEEN MET:"
                    output2 = "%f %s %f" % (total, arg, comp_val)
                else:  # False
                    output1 = "FAILURE: SYSTEM-LEVEL REQUIREMENTS HAVE NOT BEEN MET:"
                    output2 = "%f is not %s %f" % (total, arg, comp_val)

            # PRINT RESULTS TO FILE. DO NOT CHANGE
            text_file.write(asset_disp_num)
            text_file.write("\n")
            text_file.write(total_str)
            text_file.write("\n")
            text_file.write(output1)
            text_file.write("\n")
            text_file.write(output2)
            text_file.write("\n")

        text_file.close()
        os.chdir(cwd)  # Change Directory
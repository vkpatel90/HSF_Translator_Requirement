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

class datalat():
    def __init__(self):
        pass
    def main(self, asset_num, comp_arg, comp_val_str):  # MAIN FUNCTION DECLARATION
        comp_val = float(comp_val_str)
        # INPUTS FOR THIS FUNCTION (All defaulted for now)
        # variable = datalatency
        # asset_num = 2 # number of assets
        # comp_arg = "LESS_THAN"
        # comp_val = 100.0 # comparison value

        #text_file = open("datalatreq.txt", "w")
        cwd = os.getcwd()
        #print(cwd)
        path = "C:/HorizonLog/Scratch"  # Data File Path per HSF
        os.chdir(path)  # Change Directory

        text_file = open("datalatreq.txt", "w")

        # Establish operator designations.
        ops = {"<": operator.lt, ">": operator.gt, "<=": operator.le, ">=": operator.ge, "==": operator.eq,
               "!=": operator.ne}

        for index in range(asset_num):  # For each asset...
            # data buffer fill ratio & number of pixes file names
            databuffer_file = "asset%d_databufferfillratio.csv" % (index + 1)
            numpixels_file = "asset%d_numpixels.csv" % (index + 1)

            databuffer_data = []  # Empty array to import buffer ratio data
            numpixels_data = []  # Empty array to import pixel numbers data

            databuff_real = []
            databuff_time = []
            numpix_real = []
            numpix_time = []

            # FETCH DATA FROM CSV FILES

            with open(databuffer_file) as bufferdata:  # Read csv data into array
                reader = csv.reader(bufferdata, delimiter=",")
                for row in reader:
                    databuffer_data.append(row)

            with open(numpixels_file) as numpixelsdata:  # Read csv data into array
                reader = csv.reader(numpixelsdata, delimiter=",")
                for row in reader:
                    numpixels_data.append(row)

            # total_buff = 0
            ind_buff = len(databuffer_data)  # Length of array (number of data points)
            # total_numpix = 0
            ind_numpix = len(numpixels_data)  # Length of array (number of data points)

            for ndx1 in range(1, ind_buff):
                part1 = databuffer_data[ndx1]  # Select Data Point
                databuff_real.append(float(part1[1]))
                databuff_time.append(float(part1[0]))

            for ndx2 in range(1, ind_numpix):
                part2 = numpixels_data[ndx2]  # Select Data Point
                numpix_real.append(float(part2[1]))
                numpix_time.append(float(part2[0]))

            # Convert data buffer fill ratio to bytes given 4098-byte SSDR
            ssdr = 4098.000  # SSDR size in bytes
            buffbytes_data = [temp1 * ssdr for temp1 in databuff_real]

            # Convert number of pixels to bytes
            pix2byte = 0.002  # Conversion factor from pixels to bytes
            pixbytes_data = [temp2 * pix2byte for temp2 in numpix_real]

            # Find the total amount of bytes downlinked
            buff_diff_total = 0
            ind_bbd_lim = len(buffbytes_data) - 1

            dl_times = []

            for ndx3 in range(0, ind_bbd_lim):
                if buffbytes_data[ndx3 + 1] < buffbytes_data[ndx3]:
                    buff_diff = buffbytes_data[ndx3] - buffbytes_data[ndx3 + 1]
                    buff_diff_total = buff_diff_total + buff_diff
                    dl_times.append(databuff_time[ndx3 + 1])

            dl_max_time = max(dl_times)

            lat_times = []
            buff_data_left = buff_diff_total
            pbd_len = len(pixbytes_data)
            # Calculate Data Latency for each downlinked image
            ndx4 = 0
            img_bytes = pixbytes_data[ndx4]
            while img_bytes < buff_data_left and ndx4 < pbd_len:
                img_bytes = pixbytes_data[ndx4]
                if img_bytes > 0.0:
                    buff_data_left = buff_data_left - img_bytes
                    lat_time_inst = dl_max_time - numpix_time[ndx4]
                    lat_times.append(lat_time_inst)
                ndx4 = ndx4 + 1

            # Average Data Latency Values
            img_dlnk = len(lat_times)
            total = sum(lat_times) / img_dlnk

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
            total_lat = "Average Data Latency of %f seconds with %d images downlinked" % (total, img_dlnk)

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

            # Print Results
            text_file.write(asset_disp_num)
            text_file.write("\n")
            text_file.write(total_lat)
            text_file.write("\n")
            text_file.write(output1)
            text_file.write("\n")
            text_file.write(output2)
            text_file.write("\n")

        text_file.close()
        os.chdir(cwd)  # Change Directory
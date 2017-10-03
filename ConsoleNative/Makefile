###########################
# Common make Variables 
###########################
CFG?=Release
ifeq ($(CFG),Release)        
  OUTDIR := Release_x86_64
  OPT := -O3
else
  CFG := Debug
  OUTDIR := Debug_x86_64
  OPT := -O0 -g3
endif

RM := rm -rf
MKDIR := mkdir -p
CXX := g++
LD := g++
EXE_NAME := console_native
.DEFAULT_GOAL := all 

############################################################################################################################
# Microsoft GesturesSDK related variables
# To build the ConsoleNative sample, you need to first copy over the header and library dependencies 
# from your Microsoft Gestures installation directory on your Windows machine to your Linux machine.
# The native part of Microsoft GesturesSDK is found at %MicrosoftGsturesInstallDir%/SDK/ and it is structured as follows:
# native
#  include/     <---- these are the headers you need to copy to your Linux machine
#  lib/
#     x64/
#		uwp
#		windows/
#		ubuntu/ <---- here reside the libraries you need to copy to your Linux machine
#		  Release/
#		  Debug/
#
# User local layout example:
# ~MicrosoftGesturesSdk
#    include/
#       <Here reside all the header files>
#    Release/
#      <Here reside all the required shared objects/libraries>
#    Debug/
#      <Here reside all the required shared objects/libraries>
############################################################################################################################
GESTURES_SDK_INCLUDE_PATH := $(HOME)/MicrosoftGesturesSdk/include
GESTURES_SDK_LIBS_PATH := $(HOME)/MicrosoftGesturesSdk/$(CFG)
GESTURES_SDK_LIBS := -lmicrosoft_gestures_proxy -lmicrosoft_gestures_common -lmicrosoft_gestures

#############
# targets
#############
$(OUTDIR):
	$(MKDIR) "$@"

$(OUTDIR)/ConsoleNative.o: ConsoleNative.cpp
	$(CXX) -I$(GESTURES_SDK_INCLUDE_PATH) $(OPT) -Wall -std=c++11 -Werror -Wextra -fmessage-length=0 -fPIC -c -o"$@" "$<"	

$(OUTDIR)/$(EXE_NAME): $(OUTDIR) $(OUTDIR)/ConsoleNative.o
	$(LD) -Wl,-rpath,$(GESTURES_SDK_LIBS_PATH) -o $(OUTDIR)/$(EXE_NAME) $(OUTDIR)/ConsoleNative.o -L$(GESTURES_SDK_LIBS_PATH) $(GESTURES_SDK_LIBS)

.PHONY:	all
all: $(OUTDIR)/$(EXE_NAME)
	
.PHONY: clean
clean:
	$(RM) $(OUTDIR)

# Invocation examples: 
# $> make REMOTE_WINDOWS_HOSTNAME=foo_windows_machine run # via hostname
# $> make REMOTE_WINDOWS_HOSTNAME=10.164.120.111 run      # via an explicit IP address
.PHONY: run
run: all
	cd $(OUTDIR) && ./$(EXE_NAME) $(REMOTE_WINDOWS_HOSTNAME)

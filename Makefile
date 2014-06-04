.PHONY: run
DWLIST := dropbox_wlist

$(DWLIST).exe: $(DWLIST).fsx
	fsharpc $(DWLIST).fsx

run: $(DWLIST).exe
	mono $(DWLIST).exe

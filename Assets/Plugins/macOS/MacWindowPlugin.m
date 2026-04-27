#import <Cocoa/Cocoa.h>

void MakeWindowTransparent() {
    NSWindow *window = [NSApp mainWindow];
    if (window != nil) {
        [window setOpaque:NO];
        [window setBackgroundColor:[NSColor clearColor]];
        [window setStyleMask:NSWindowStyleMaskBorderless];
        [window setLevel:NSFloatingWindowLevel]; // Keep on top
        [window setHasShadow:NO];
    }
}

void SetWindowClickThrough(bool clickThrough) {
    NSWindow *window = [NSApp mainWindow];
    if (window != nil) {
        [window setIgnoresMouseEvents:clickThrough];
    }
}

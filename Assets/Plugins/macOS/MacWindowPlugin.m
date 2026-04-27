#import <Cocoa/Cocoa.h>
#import <CoreGraphics/CoreGraphics.h>

typedef bool (*HitTestCallback)(int, int);

static CFMachPortRef gEventTap = NULL;
static CFRunLoopSourceRef gRunLoopSource = NULL;
static HitTestCallback gHitTest = NULL;
static pid_t gPid = 0;
static bool gDragging = false;

static inline NSWindow *GetUnityMainWindow() {
    return [NSApp mainWindow];
}

static inline float GetBackingScale() {
    NSScreen *screen = [NSScreen mainScreen];
    if (screen == nil) return 1.0f;
    return (float)[screen backingScaleFactor];
}

static CGEventRef TapCallback(CGEventTapProxy proxy, CGEventType type, CGEventRef event, void *userInfo) {
    if (gHitTest == NULL) return event;

    if (type == kCGEventTapDisabledByTimeout || type == kCGEventTapDisabledByUserInput) {
        if (gEventTap != NULL) CGEventTapEnable(gEventTap, true);
        return event;
    }

    CGEventType t = type;
    if (t != kCGEventLeftMouseDown && t != kCGEventLeftMouseDragged && t != kCGEventLeftMouseUp) return event;

    CGPoint loc = CGEventGetLocation(event);
    float scale = GetBackingScale();
    int x = (int)(loc.x * scale);
    int y = (int)(loc.y * scale);

    bool hit = gHitTest(x, y);
    if (gDragging) hit = true;

    NSWindow *window = GetUnityMainWindow();
    if (window == nil) return event;

    if (t == kCGEventLeftMouseDown) {
        if (hit) {
            gDragging = true;
            [window setIgnoresMouseEvents:NO];
            CGEventPostToPid(gPid, event);
            return NULL;
        }
        return event;
    }

    if (gDragging && (t == kCGEventLeftMouseDragged || t == kCGEventLeftMouseUp)) {
        CGEventPostToPid(gPid, event);
        if (t == kCGEventLeftMouseUp) {
            gDragging = false;
            [window setIgnoresMouseEvents:YES];
        }
        return NULL;
    }

    return event;
}

void MakeWindowTransparent() {
    NSWindow *window = GetUnityMainWindow();
    if (window != nil) {
        [window setOpaque:NO];
        [window setBackgroundColor:[NSColor clearColor]];
        [window setStyleMask:NSWindowStyleMaskBorderless];
        [window setLevel:NSFloatingWindowLevel];
        [window setHasShadow:NO];
        [window setIgnoresMouseEvents:NO];
    }
}

void InstallClickThroughHitTest(void *hitTestCallback) {
    gPid = [[NSProcessInfo processInfo] processIdentifier];
    gHitTest = (HitTestCallback)hitTestCallback;

    if (gEventTap != NULL) return;

    CGEventMask mask = CGEventMaskBit(kCGEventLeftMouseDown) |
                       CGEventMaskBit(kCGEventLeftMouseDragged) |
                       CGEventMaskBit(kCGEventLeftMouseUp);

    gEventTap = CGEventTapCreate(kCGSessionEventTap,
                                 kCGHeadInsertEventTap,
                                 kCGEventTapOptionDefault,
                                 mask,
                                 TapCallback,
                                 NULL);

    if (gEventTap == NULL) {
        NSWindow *window = GetUnityMainWindow();
        if (window != nil) [window setIgnoresMouseEvents:NO];
        return;
    }

    gRunLoopSource = CFMachPortCreateRunLoopSource(kCFAllocatorDefault, gEventTap, 0);
    CFRunLoopAddSource(CFRunLoopGetMain(), gRunLoopSource, kCFRunLoopCommonModes);
    CGEventTapEnable(gEventTap, true);

    NSWindow *window = GetUnityMainWindow();
    if (window != nil) [window setIgnoresMouseEvents:YES];
}

void UninstallClickThroughHitTest() {
    gHitTest = NULL;
    gDragging = false;

    if (gRunLoopSource != NULL) {
        CFRunLoopRemoveSource(CFRunLoopGetMain(), gRunLoopSource, kCFRunLoopCommonModes);
        CFRelease(gRunLoopSource);
        gRunLoopSource = NULL;
    }

    if (gEventTap != NULL) {
        CFMachPortInvalidate(gEventTap);
        CFRelease(gEventTap);
        gEventTap = NULL;
    }
}

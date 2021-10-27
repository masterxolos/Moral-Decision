//
//  TTPATTDisclaimer.m
//  Unity-iPhone
//
//  Created by ShmulikA on 04/05/2021.
//

#import "TTPUnityServiceManager.h"
#import "ATTDisclaimerCallback.h"

extern "C"
{
    void ttpAttDisclaimer()
    {
        UIAlertController* alert = [UIAlertController
                        alertControllerWithTitle:@"Ads keep this game free"
                                         message:@"On the next popup, allow tracking to help support the game and keep it free for everyone. It will allow us to provide you with the best personalized ads for you."
                                  preferredStyle:UIAlertControllerStyleAlert];

        UIAlertAction* yesButtonAction = [UIAlertAction actionWithTitle:@"Next"
                                                                  style:UIAlertActionStyleDefault
                                                                handler:^(UIAlertAction* action)
        {
            [ATTDisclaimerCallback notify];
        }];
        [alert addAction:yesButtonAction];
        
        TTPServiceManager* serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrootViewController> rootViewController = [serviceManager get:@protocol(TTPIrootViewController)];
        UIViewController* vc = [rootViewController get];
        if (vc != nil)
        {
            [vc presentViewController:alert animated:YES completion:nil];
        }
    }
}

This software is no longer actively developed.

Consider checking out [Artemis](https://github.com/Artemis-RGB/Artemis) for a even more feature rich replacement.

# KeyboardAudioVisualizer
It's colorful - I like it!

**Example-Video:**   
[![Example-Video](https://img.youtube.com/vi/mby2NYN0V1o/0.jpg)](https://www.youtube.com/watch?v=mby2NYN0V1o)

## Visualizations
#### Keyboard
- **"Frequency Bars"** - Simple spectrum visualizer.
- **"Level"** - Shows the overall volume.
- **"Beat detection"** - Pulses to the beat of the music. (This isn't working really well right now, depending on the music. But in general not satisfying, sorry.)
#### Mouse/Headset
- **"Beat detection"** - Pulses to the beat of the music. (This isn't working really well right now, depending on the music. But in general not satisfying, sorry.)
#### Mousepad/Lightbar (K95 Platinum)
- **"Level"** - Shows the overall volume.
- **"Beat detection"** - Pulses to the beat of the music. (This isn't working really well right now, depending on the music. But in general not satisfying, sorry.)

## Supported devices
- All Corsair RGB-devices.
- Logitech G910 and G610 with physical EU layout. (Untested but should work)
- (In theory every device with SDK support could be include, open an issue if you want to help with increasing the range of supported devices!)

## Settings
#### Frequency Bars
- **Spectrum:** The way the spectrum is grouped together. Values are: _(default: Logarithmic)_
  - **_Linear_**: Each bar represents the same range of frequencies. Most of the time this doesn't look good since low frequencies are underrepresented.
  - **_Logarithmic_**: The higher the frequencies get the wider the range of grouped frequencies. This is close to the way humans perceive sound and therfore most of the time looks quite good as long as the range of used frequencies is big enough.
  - **_Gamma_**: While Gamma-correction is known from image-processing it still applies quite well to audio-data, grouping not as extreme as logarithmic but still better than linear. The used Gamma-value can be configured.
- **Value:** The way the value of a frequency bar is determined. Values are: _(default: Sum)_
  - **_Sum_**: Sums the power of all frequencies grouped in the bar using all available data. Combining this with logarithmic grouping gives the most realistic representation.
  - **_Max_**: Uses the maximum value in each group making sure peaks are caught well. This works quite good with gamma-grouping.
  - **_Average_**: Uses the average of all frequencies grouped in the bar. This smooths out the whole graph quite a lot.
- **Bars:** The number of bars used to represent the spectrum. _(default: 48)_
- **Min Freq.:** The minimum frequency used in the graph. This value shouldn't be modified. _(default: 60)_
- **Max Freq.:** The maximum frequency used in the graph. This value can be lowered to increase the value of lower frequencies. Using high values might lead to death bars if the audio is mastered with an low-pass filter cutting high frequencies. _(default: 15000)_
- **Gamma:** The correction value used for gamma-grouping (disabled if any other grouping is selected). High values lead to a stronger compression of high frequencies. _(default: 2)_
- **Reference:** The reference value used to calculate the power of each bar. Adjust this to your audio volume. Low volume -> low reference, high volume -> higher reference. _(default: 90)_
- **Smoothing:** Smooths the graph to prevent flickering. Low values will cause a hectic fast plot, high values a slow one without peaks. _(default: 3)_
- **Emphasize:** Emphasizes peaks. The higher the value, the bigger the difference between a "loud-bar" and a "quiet-bar". _(default: 0.5)_
   
**Equalizer**   
Allows to finetune the graph by slective increasing/decresing the value.
You can add new pivots by rightclicking on the visualization-window.   
Existing pivots can be deleted by rightclicking on them or moved by leftclicking and dragging around.   


#### Beat detection
_No configuration right now_

#### Level
- **Calculation:** Defines how the RMS of the audio is plotted. Values are _Linear_, _Logarithmic_ and _Exponential_. The used range of the plott increases in that order (exponential has the widest range of peaks). _(default: Logarithmic)_

- **Scale:** Scales the whole graph. Use this to to fit your audio volume. _(default: 3)_

- **Smoothing:** Smooths the plot to prevent flickering. Low values will cause a hectic fast plot, high values a slow one without peaks. _(default: 8)_

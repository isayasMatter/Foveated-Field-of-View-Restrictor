# Foveated Field of View Restrictor

This project contains a simple implementation of the foveated Field-of-View (FOV) restrictor discussed in our IEEEVR'20 paper published [here](https://doi.org/10.1109/VR46266.2020.00087).

## What is a Foveated FOV Restrictor

Virtual reality sickness typically results from visual-vestibular conflict. Because self-motion from optical flow is driven most strongly by motion at the periphery of the retina, 
reducing the user’s field-of-view (FOV) during locomotion has proven to be an effective strategy to minimize visual vestibular conflict and VR sickness. 
Current FOV restrictor implementations reduce the user’s FOV by rendering a restrictor whose center is fixed at the center of the head mounted display (HMD), 
which is effective when the user’s eye gaze is aligned with head gaze. However, during eccentric eye gaze, users may look at the FOV restrictor itself, 
exposing them to peripheral optical flow which could lead to increased VR sickness. To address these limitations, we developed a foveated FOV restrictor 
and we explore the effect of dynamically moving the center of the FOV restrictor according to the user’s eye gaze position. The animation below shows the difference between a 
fixed (dynamic) restrictor and our foveated restrictor.

<p align="center">
  <img src="https://github.com/isayasMatter/Foveated-Field-of-View-Restrictor/blob/master/Assets/Images/FoveatedFOVRestrictor.gif" alt="animated" alt="Comparison between a fixed and foveated restrictor"/>
</p>

## Implementation

### Dynamic restriction

To implement a foveated FOV restrictor (FV) that responds to the user’s eye gaze,  we first implement a fixed FOV restrictor to dynamically manipulate the FOV in response 
to changes in the participant’s linear and angular velocities in the virtual environment. After the fixed restrictor we implement a method to manipulate it’s position in 
the VE based on the user’s eye gaze position.For both FOV restriction conditions, the FOV is decreased as the participant’s speed or angular velocity increases.
To restrict the FOV, we used a black texture with a fully transparentcircular cut-off. The circular cut-off is defined by an inner and outerradius that together form an annulus. 
We call the region betweenthese two radii the feathering region. In this region the opacity ofthe circular cut-off increases linearly from completely transparentto completely 
opaque.  The inner radius of the circular cut-off iscalculated using the following formula.

<p align="center">
  <img src="https://latex.codecogs.com/svg.latex?FOV_{r,t}&space;=&space;FOV_{r,t-1}&space;\times&space;[1&space;-&space;(RF_{max}&space;\times&space;max(\frac{v_t}{v_{max}},&space;\frac{\omega_t}{\omega_{max}}))]" />
 </p>
 
 For details of the variables included in this formula, please check our paper [here](https://par.nsf.gov/servlets/purl/10193017).
 
 ### Eye gaze signal smoothing and filtering
 
The movement of the foveated restrictor must, ideally, be smooth and have low latency in order to be imperceptible by the user. However, eye tracking signals contain inherent noise (jitter) and various other signal artifacts that cause sudden spikes in the signal. The raw signal's noise can make the restrictor jittery, and the spikes can cause jumps in the restrictor. To avoid this, we must filter and smooth the signal. Our code includes implementations of a low pass filter to remove high frequency noise and two types of smoothing agorithms, running average smoothing and spring damping, to remove sudden spikes from the signal. The filtering and smoothing steps can introduce perceptible delay into the signal and must be used conservatively.


 
 

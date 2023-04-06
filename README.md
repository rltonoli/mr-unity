# mr-unity

Unity/C# implementation of the dissertation: “Motion Retargeting Preserving Spatial Relationship”
  - Motion retargeting algorithm that preserves the distance and orientation relationship of the hands with the body surface.
  - Participants of a perceptual evaluation found that retargeted animations with the proposed method better matched the original motions.

Available at: https://bit.ly/3lfh0FQ
Rodolfo Luis Tonoli
Supervisor: Profa. Dra. Paula Dornhofer Paro Costa

# Work plan
**BVH File**  (utils)
- ~~Read~~ 
- Write  
- ~~Animation and joints data structure~~
- ~~Draw bvh file animation (source)~~

**Motion Retargeting** (chapter 3)
- **Initial Motion Retargeting** (3.1)
  - **Skeleton map** (3.1.2)
    - ~~Source~~
    - ~~Target~~
    - Draw skeleton map
  - ~~Motion retargeting aligning bones~~ (3.1.3)
  - ~~Motion retargeting applying global rotation~~ (3.1.3)
  - Solve root translation (3.1.4)
- **Spatial relationship encoding** (3.2)
  - ~~Read and draw source surface calibration~~ (3.2.1)
  - ~~Read and draw target surface calibration~~ (3.2.1)
  - Computing egocentric coordinates (3.2.2)
    - ~~Reference point and displacement vector~~
    - ~~Importance orthogonality and proximity~~
    - Kinematic path normalization
    - Relative joints orientation
    - Draw coords
- **Pose adaptation** *,i.e., motion retargeting* (3.3)
  - **Compute target positions** (3.3.1)
    - Kinematic path
    - Normalization factor
    - *New* displacement vectors and target reference points
    - Compute target animation's joint positions
    - Draw coords
    - **Pose adjustment** *with IK* (3.3.2)
      - Draw old and new joint positions and end effectors' target
    - **Adjust joint orientation** (3.3.3)
		- Draw old and new joint orientation

**Other ~~cool(?)~~ things**
	- Add other performer's animations as source
	- Add different characters as target

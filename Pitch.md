## Pitch Script

## Zach About Problem and Simulation
Thousands of people die at sea each year because rescuers can't reach them in time.

Watson Search and Rescue uses Watson's Visual Recognition API with a realistic
simulation to model how machine learning and a drone swarm could aid rescue
efforts at sea.

What's the real bottleneck on how drones can help rescuers? Watching footage.
To closely watch live drone footage for anomalies in the water, you'd need many
human operators all tuned in at the same time. What if we could cut the signal from the noise?

We modeled a 1km square of open ocean to scale in Unity. Then we made a swarm of
camera-mounted drones.

Somewhere in the open ocean is an overturned boat.

Here's what that looks like.

These drones traverse the open ocean like a search party, sending images and their coordinates
every 100 meters to the Watson Visual Recognition API.

Here's what that looks like. They produce images like these.

We trained a Watson custom classifier on sample images of aquatic accidents
versus open ocean.

Watson filters out images of open ocean.

But when Watson spots an image that might be an emergency, it sends it to a
simple dashboard built with Loopback.

## Sam on Dashboard
Humans have the final say about what coordinates are likely to have an emergency.

Occasionally false positives appear on the dashboard, which the operator can easily spot.

A human operator sees any photos flagged as potentially having anomalies, looks at them, and can click them.

When the user flags a photo as an emergency, the dashboard forwards the image to Watson as a hit, improving the classifier. The user's click also sends coordinates to the drone swarm to converge on that location.

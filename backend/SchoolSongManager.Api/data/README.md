# Data Directory

This directory contains the persistent data for the SchoolSongManager application.

**Important**: This directory should not be overwritten during deployments to preserve the song library and historical data.

## Structure
- `songs/` - Song metadata and files
- `themes/` - Theme definitions  
- `history/` - Usage history records
- `uploads/` - Uploaded audio and image files

## Deployment Note
Ensure your deployment pipeline preserves this directory structure and its contents.
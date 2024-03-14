# Mindenit Schedule API
This repository is a new project of the schedule API, with Docker support, and more optimization, reworking some logic for more stability, and just working on bugs.

### Build
Build the image with the following commands:
```bash
    git clone [repo link]
    cd api
    docker build -t mindenit-schedule-api .
```

### Run
Run builded image for access the API:
```bash
    docker run -d -p 3000:8080 --name API mindenit-schedule-api
```

And now you can access the API on `http://localhost:3000/`
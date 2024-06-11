# Mindenit Schedule API
This repository is a new project of the schedule API, with Docker support, and more optimization, reworking some logic for more stability, and just working on bugs.

### Build
Build the image with the following commands:
```bash
    git clone https://github.com/mindenit/api.git
    cd api
    docker build -t api .
```

### Run
Run compose for access the API:
```bash
    docker compose up
```

And now you can access the API on `http://localhost:4173/`
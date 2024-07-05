
# Project is Archived 

While the project is no longer under active maintenance and is read-only, it's still possible to clone or fork the repository. [Further information available here](https://docs.github.com/en/repositories/archiving-a-github-repository/archiving-repositories). For any issues regarding the "Archived" status of the repository, you may reach out to LaunchGemini.

# Introduction

The MedData-Creation transforms medical datasets in DICOM-RT format to NIFTI. The converted datasets are ready for consumption by the InnerEye-DeepLearning library.

## Features

The tool's core features include:
- Dataset Resampling to a common voxel size
- Ground truth structures' renaming
- Making structures mutually exclusive
- Missing structures? The tool creates empty ones
- Discarding subjects without required structures
- DataSet augmentation by combining multiple structures
- Removing structure parts based on z coordinate
- Computing dataset statistics

Please follow the instructions given below for Installation and Usage details. For contributors, please refer to the 'Contributing' section at the end.

## Contributing

Contributions and suggestions to this project are welcomed. Nonetheless, most contributions need you to accept a Contributor License Agreement (CLA) giving us the rights to use your contribution. For detailed information, please visit <https://cla.opensource.microsoft.com>.

Once you raise a pull request, an automated CLA bot will guide you through the CLA process. Have in mind that the CLA process is only once for all repositories that use our CLA.

Adherence to the [LaunchGemini Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/) is required. See the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or communicate with [LaunchGemini](mailto:LaunchGemini@gmail.com) if you have further questions or comments.
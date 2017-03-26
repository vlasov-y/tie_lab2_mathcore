﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIE_Lab2_MathCore
{
    public class TIE_Lab2_MathCore
    {
        private bool _is_probabilities_generated;
        private List<List<double>> _probabilites;

        public TIE_Lab2_MathCore()
        {
            _probabilites = new List<List<double>>();
            _is_probabilities_generated = false;
        }

        public TIE_Lab2_MathCore(UInt16 matrix_size)
        {
            if (matrix_size == 0)
            {
                throw new Exception("Matrix size cannot be zero.");
            }
            // Initialize matrix.
            for (UInt16 index = 0; index < matrix_size; ++index)
            {
                _probabilites.Add(new List<double>((Int32)matrix_size));
            }
            // Filling it.
            GenerateNewProbabilities();
        }

        public void ChangeMatrixSize(UInt16 matrix_size)
        {
            if (matrix_size == 0)
            {
                throw new Exception("Matrix size cannot be zero.");
            }
            // Refill matrix.
            _probabilites.Clear();
            for (UInt16 index = 0; index < matrix_size; ++index)
            {
                _probabilites.Add(new List<double>((Int32)matrix_size));
            }
        }

        private void CheckMatrixSizeSet()
        {
            if (_probabilites.Count == 0)
            {
                throw new Exception("You have not initialized size of probability matrix.");
            }
        }

        private void CheckProbabilitiesGenerated()
        {
            CheckMatrixSizeSet();
            if (!_is_probabilities_generated)
            {
                throw new Exception("You have not fill probability matrix.");
            }
        }

        private List<double> GenerateRandomProbabilitiesList(int length)
        {
            if (length <= 0)
            {
                throw new Exception("List length cannot be [ -le 0 ].");
            }

            List<double> probabilities = new List<double>(length);
            // Generate random borderse in interval between 0 and 1 and small intervals will be needed.
            Random random = new Random();
            List<double> borders = new List<double>(length-1);
            for (int random_border = 0; random_border < borders.Count; random_border++)
            {
                borders[random_border] = random.NextDouble();
            }
            borders.Sort();
            // Calculating intervals.
            double summary = 0;
            for (int border_index = 0; border_index < borders.Count - 1; border_index++)
            {
                probabilities.Add(borders[border_index + 1] - borders[border_index]);
                summary += probabilities.Last();
            }
            probabilities.Add(1.0d - summary);

            return probabilities;
        }

        public IEnumerable<ICollection<double>> GenerateNewProbabilities()
        {
            CheckMatrixSizeSet();
            // Generating different p(x).
            List<double> p_x_list = GenerateRandomProbabilitiesList(_probabilites.Count);
            
            for (int row_index = 0; row_index < _probabilites.Count; ++row_index)
            {
                _probabilites[row_index].Clear();
                List<double> percentages = GenerateRandomProbabilitiesList(_probabilites.Count);
                // Smashing x_i by percent parts.
                for (int percent_index = 0; percent_index < percentages.Count; percent_index++)
                {
                    percentages[percent_index] *= p_x_list[percent_index];
                }
                _probabilites[row_index].AddRange(percentages);
            }
            _is_probabilities_generated = true;
            return _probabilites;
        }

        private List<List<double>> CreateResultMatrix()
        {
            Int32 matrix_size = _probabilites.Count;
            List<List<double>> result_matrix = new List<List<double>>();
            // Creating new matrix.
            for (UInt16 index = 0; index < matrix_size; ++index)
            {
                result_matrix.Add(new List<double>(matrix_size));
            }
            return result_matrix;
        }

        public IEnumerable<ICollection<double>> PAB()
        {
            CheckProbabilitiesGenerated();
            return _probabilites;
        }

        public double HAB()
        {
            return HA() - HAsB();
        }

        public double HAsB()
        {
            double result = 0;
            List<double> hasbi = (List<double>)HAsBi();
            List<double> pi = (List<double>)Pi(HSource.B);

            for (int i = 0; i < hasbi.Count; i++)
            {
                result += hasbi[i] * pi[i];
            }

            return result * -1.0d;
        }

        private enum HsSource
        {
            AsB,
            BsA
        }

        private ICollection<double> Hs(HsSource source)
        {
            List<double> result_matrix = new List<double>(_probabilites.Count);
            // TODO Implement HBsA
            double sum = 0;
            for (int col = 0; col < _probabilites.Count; col++)
            {
                for (int row = 0; row < _probabilites.Count; row++)
                {
                    switch (source)
                    {
                        case HsSource.AsB:
                            sum += _probabilites[col][row] * Math.Log(_probabilites[col][row], 2);
                            break;
                        case HsSource.BsA:
                            sum += _probabilites[row][col] * Math.Log(_probabilites[row][col], 2);
                            break;
                        default:
                            throw new Exception("Algorithm for calculation from new source is undefined.");
                    }
                }
                result_matrix[col] = sum * -1.0d;
            }
            return result_matrix;
        }

        public ICollection<double> HAsBi()
        {
            return Hs(HsSource.AsB);
        }

        public ICollection<double> HBsAi()
        {
            return Hs(HsSource.BsA);
        }

        private enum HSource
        {
            A,
            B
        }

        private double H(HSource source)
        {
            // Calculating all p(x_i)
            List<double> p_x = (List<double>)Pi(source);
            // Calculating H
            double sum = 0;
            for (int i = 0; i < p_x.Count; i++)
            {
                sum += p_x[i] * Math.Log(p_x[i], 2);
            }

            return sum * -1.0d;
        }

        public double HA()
        {
            return H(HSource.A);
        }

        public double HB()
        {
            return H(HSource.B);
        }

        private ICollection<double> Pi(HSource source)
        {
            // Calculating all p(x_i)
            double sum = 0;
            List<double> p_x = new List<double>();
            for (int col = 0; col < _probabilites.Count; col++)
            {
                sum = 0;
                for (int row = 0; row < _probabilites.Count; row++)
                {
                    switch (source)
                    {
                        case HSource.A:
                            sum += _probabilites[row][col];
                            break;
                        case HSource.B:
                            sum += _probabilites[col][row];
                            break;
                        default:
                            throw new Exception("Algorithm for calculation from new source is undefined.");
                    }
                }
                p_x.Add(sum);
            }

            return p_x;
        }

        public ICollection<double> PiA()
        {
            return Pi(HSource.A);
        }

        public ICollection<double> PiB()
        {
            return Pi(HSource.B);
        }
    }
}

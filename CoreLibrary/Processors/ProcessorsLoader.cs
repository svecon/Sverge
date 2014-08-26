﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors
{
    public class ProcessorsLoader : IProcessorLoader
    {

        SortedList<int, IPreProcessor> PreProcessors;

        SortedList<int, IProcessor> Processors;

        SortedList<int, IPostProcessor> PostProcessors;

        public ProcessorsLoader()
        {
            PreProcessors = new SortedList<int, IPreProcessor>();
            Processors = new SortedList<int, IProcessor>();
            PostProcessors = new SortedList<int, IPostProcessor>();
        }

        public void Load()
        {
            AddProcessor(new SizeTimeDiffProcessor());
            AddProcessor(new BinaryDiffProcessor());
        }

        public void AddProcessor(IPreProcessor processor)
        {
            try
            {
                PreProcessors.Add(processor.Priority, processor);
            } catch (ArgumentException e) {
                throw new ArgumentException("Priority collision! Please pick different Priority for " + processor, e);
            }
        }

        public void AddProcessor(IProcessor processor)
        {
            try
            {
                Processors.Add(processor.Priority, processor);
            } catch (ArgumentException e)
            {
                throw new ArgumentException("Priority collision! Please pick different Priority for " + processor, e);
            }
        }

        public void AddProcessor(IPostProcessor processor)
        {
            try
            {
                PostProcessors.Add(processor.Priority, processor);
            } catch (ArgumentException e)
            {
                throw new ArgumentException("Priority collision! Please pick different Priority for " + processor, e);
            }
        }

        public IEnumerable<IPreProcessor> GetPreProcessors()
        {
            foreach (var processor in PreProcessors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerable<IProcessor> GetProcessors()
        {
            foreach (var processor in Processors)
            {
                yield return processor.Value;
            }
        }

        public IEnumerable<IPostProcessor> GetPostProcessors()
        {
            foreach (var processor in PostProcessors)
            {
                yield return processor.Value;
            }
        }

    }
}
